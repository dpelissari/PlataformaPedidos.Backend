using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PlataformaPedidos.Infraestrutura.Mensageria;

namespace PlataformaPedidos.TestesIntegracao;

[Collection(nameof(AmbienteIntegracao))]
public sealed class PedidosApiTestes(AmbienteIntegracao ambiente)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly Guid ClienteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProdutoId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task PostPedidos_ComDadosValidos_CriaEPermiteConsultar()
    {
        await ambiente.LimparAsync();

        var criado = await CriarPedidoAsync();

        criado.Status.Should().Be("Rascunho");
        criado.Logradouro.Should().Be("Avenida Paulista");
        criado.Numero.Should().Be("1000");
        criado.ValorTotal.Should().Be(21m);

        var obtido = await ambiente.Cliente.GetFromJsonAsync<PedidoTeste>($"/api/pedidos/{criado.Id}", Json);
        obtido!.Id.Should().Be(criado.Id);
        obtido.Status.Should().Be("Rascunho");

        var lista = await ambiente.Cliente.GetFromJsonAsync<List<PedidoTeste>>("/api/pedidos", Json);
        lista.Should().ContainSingle(pedido => pedido.Id == criado.Id);

        var inexistente = await ambiente.Cliente.GetAsync($"/api/pedidos/{Guid.CreateVersion7()}");
        var corpo = await inexistente.Content.ReadAsStringAsync();
        inexistente.StatusCode.Should().Be(HttpStatusCode.NotFound, corpo);
        inexistente.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problema = JsonSerializer.Deserialize<Problema>(corpo, Json);
        problema!.Codigo.Should().Be("pedido.nao_encontrado");
    }

    [Fact]
    public async Task GetEndereco_NaSegundaConsulta_UsaCacheRedis()
    {
        await ambiente.LimparAsync();

        var primeira = await ambiente.Cliente.GetFromJsonAsync<EnderecoTeste>("/api/enderecos/01310100", Json);
        var segunda = await ambiente.Cliente.GetFromJsonAsync<EnderecoTeste>("/api/enderecos/01310100", Json);

        primeira!.Logradouro.Should().Be("Avenida Paulista");
        segunda!.Cidade.Should().Be("São Paulo");
        (await ambiente.ContarRequisicoesAsync("/cep/01310100")).Should().Be(1);
    }

    [Fact]
    public async Task PostPedidos_QuandoCepResponde503_RecuperaComRetry()
    {
        await ambiente.LimparAsync();
        await ambiente.InstalarCepInstavelAsync();

        var criado = await CriarPedidoAsync();

        criado.Status.Should().Be("Rascunho");
        (await ambiente.ContarRequisicoesAsync("/cep/01310100")).Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task SolicitarPagamento_PublicaOutboxEConfirmaSemReprocessarDuplicata()
    {
        await ambiente.LimparAsync();
        var criado = await CriarPedidoAsync();

        var solicitacao = await ambiente.Cliente.PostAsync($"/api/pedidos/{criado.Id}/solicitar-pagamento", null);
        var corpoSolicitacao = await solicitacao.Content.ReadAsStringAsync();
        solicitacao.StatusCode.Should().Be(HttpStatusCode.OK, corpoSolicitacao);

        var confirmado = await EsperarStatusAsync(criado.Id, "Confirmado");
        confirmado.Status.Should().Be("Confirmado");
        (await ambiente.ContarRequisicoesAsync("/pagamentos/autorizar")).Should().Be(1);

        await using var contexto = ambiente.CriarContexto();
        var mensagem = await contexto.MensagensOutbox
            .AsNoTracking()
            .SingleAsync(item => item.Topico == TopicosPedido.PagamentoSolicitado);
        (await contexto.MensagensProcessadas.CountAsync(item => item.MensagemId == mensagem.Id)).Should().Be(1);

        await RepublicarAsync(mensagem.Topico, criado.Id, mensagem.Conteudo, mensagem.Id);
        await EsperarConsumidorAsync(ambiente.Kafka);

        await using var contextoDepois = ambiente.CriarContexto();
        (await contextoDepois.MensagensProcessadas.CountAsync(item => item.MensagemId == mensagem.Id)).Should().Be(1);
        (await ambiente.ContarRequisicoesAsync("/pagamentos/autorizar")).Should().Be(1);

        var aposDuplicata = await ambiente.Cliente.GetFromJsonAsync<PedidoTeste>($"/api/pedidos/{criado.Id}", Json);
        aposDuplicata!.Status.Should().Be("Confirmado");
    }

    private async Task<PedidoTeste> CriarPedidoAsync()
    {
        var corpo = new
        {
            ClienteId,
            Cep = "01310100",
            Numero = "1000",
            Complemento = "apto 42",
            Itens = new[]
            {
                new
                {
                    ProdutoId,
                    NomeProduto = "Caderno",
                    Quantidade = 2,
                    PrecoUnitario = 10.50m
                }
            }
        };

        var resposta = await ambiente.Cliente.PostAsJsonAsync("/api/pedidos", corpo, Json);
        var texto = await resposta.Content.ReadAsStringAsync();
        resposta.StatusCode.Should().Be(HttpStatusCode.Created, texto);
        resposta.Headers.Location.Should().NotBeNull();

        var pedido = JsonSerializer.Deserialize<PedidoTeste>(texto, Json);
        pedido.Should().NotBeNull();
        resposta.Headers.Location!.ToString().Should().Contain(pedido!.Id.ToString());
        return pedido;
    }

    private async Task<PedidoTeste> EsperarStatusAsync(Guid pedidoId, string status)
    {
        var limite = DateTime.UtcNow.AddSeconds(60);
        PedidoTeste? ultimo = null;
        while (DateTime.UtcNow < limite)
        {
            var resposta = await ambiente.Cliente.GetAsync($"/api/pedidos/{pedidoId}");
            var texto = await resposta.Content.ReadAsStringAsync();
            resposta.StatusCode.Should().Be(HttpStatusCode.OK, texto);
            ultimo = JsonSerializer.Deserialize<PedidoTeste>(texto, Json);
            if (ultimo?.Status == status)
                return ultimo;

            await Task.Delay(200);
        }

        throw new TimeoutException($"O pedido permaneceu em {ultimo?.Status}.");
    }

    private async Task RepublicarAsync(string topico, Guid pedidoId, string conteudo, Guid mensagemId)
    {
        using var produtor = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = ambiente.Kafka,
            Acks = Acks.All
        }).Build();

        var mensagem = new Message<string, string>
        {
            Key = pedidoId.ToString(),
            Value = conteudo,
            Headers = new Headers()
        };
        mensagem.Headers.Add(ProdutorEventosKafka.HeaderMensagemId, Encoding.UTF8.GetBytes(mensagemId.ToString()));
        await produtor.ProduceAsync(topico, mensagem);
    }

    private static async Task EsperarConsumidorAsync(string servidores)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = servidores }).Build();
        using var observador = new ConsumerBuilder<Ignore, Ignore>(new ConsumerConfig
        {
            BootstrapServers = servidores,
            GroupId = "observador-" + Guid.NewGuid().ToString("N"),
            EnableAutoCommit = false
        }).Build();

        var limite = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < limite)
        {
            var metadados = admin.GetMetadata(TopicosPedido.PagamentoSolicitado, TimeSpan.FromSeconds(5));
            var topico = metadados.Topics.Single();
            if (topico.Error.IsError || topico.Partitions.Count == 0)
            {
                await Task.Delay(200);
                continue;
            }

            var particoes = topico.Partitions
                .Select(particao => new TopicPartition(TopicosPedido.PagamentoSolicitado, particao.PartitionId))
                .ToList();

            List<ListConsumerGroupOffsetsResult> grupos;
            try
            {
                grupos = await admin.ListConsumerGroupOffsetsAsync(
                    [new ConsumerGroupTopicPartitions(ConsumidorPagamentoPedido.Grupo, particoes)]);
            }
            catch (KafkaException)
            {
                await Task.Delay(200);
                continue;
            }

            var offsets = grupos.Single().Partitions;
            var emDia = particoes.All(particao =>
            {
                var marcas = observador.QueryWatermarkOffsets(particao, TimeSpan.FromSeconds(5));
                if (marcas.High.Value <= 0)
                    return false;

                var commit = offsets.FirstOrDefault(item =>
                    item.Topic == particao.Topic && item.Partition == particao.Partition);
                return commit is not null && !commit.Error.IsError && commit.Offset >= marcas.High;
            });

            if (emDia)
                return;

            await Task.Delay(200);
        }

        throw new TimeoutException("O consumidor de pagamento não alcançou o fim do tópico.");
    }

    private sealed record PedidoTeste(Guid Id, string Status, string Logradouro, string Numero, decimal ValorTotal);

    private sealed record EnderecoTeste(string Cep, string Logradouro, string Bairro, string Cidade, string Estado);

    private sealed record Problema(string? Title, string? Detail, int? Status, string? Codigo);
}
