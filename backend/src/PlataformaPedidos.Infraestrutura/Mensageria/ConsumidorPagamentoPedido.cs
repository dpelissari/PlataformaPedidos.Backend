using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Pedidos;
using PlataformaPedidos.Infraestrutura.Observabilidade;
using PlataformaPedidos.Infraestrutura.Persistencia;

namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Consome o pagamento solicitado, autoriza no gateway e confirma ou cancela o pedido uma única vez.
/// </summary>
public sealed class ConsumidorPagamentoPedido(
    IServiceScopeFactory escopos,
    IOptions<OpcoesKafka> opcoes,
    ILogger<ConsumidorPagamentoPedido> logger)
{
    /// <summary>
    /// Grupo usado para confirmar o offset só depois do processamento.
    /// </summary>
    public const string Grupo = "plataforma-pedidos.pagamento";

    public const string Nome = "ConsumidorPagamentoPedido";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Lê o tópico até o cancelamento do host. Falha transitória não confirma o offset.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task ExecutarAsync(CancellationToken cancellationToken)
    {
        using var consumidor = CriarConsumidor();
        consumidor.Subscribe(TopicosPedido.PagamentoSolicitado);

        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? resultado;
            try
            {
                resultado = consumidor.Consume(TimeSpan.FromMilliseconds(500));
            }
            catch (ConsumeException excecao)
            {
                logger.LogWarning(excecao, "Falha ao ler o tópico de pagamento solicitado.");
                continue;
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (resultado is null)
                continue;

            try
            {
                var confirmar = await ProcessarAsync(resultado, cancellationToken);
                if (!confirmar)
                {
                    await AguardarAsync(cancellationToken);
                    continue;
                }

                consumidor.Commit(resultado);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception excecao)
            {
                logger.LogError(excecao, "Não foi possível processar a mensagem de pagamento {Offset}.", resultado.Offset);
                await AguardarAsync(cancellationToken);
            }
        }
    }

    private async Task<bool> ProcessarAsync(
        ConsumeResult<string, string> resultado,
        CancellationToken cancellationToken)
    {
        if (!TentarLer(resultado, out var mensagemId, out var pedidoId, out var valor, out var moeda, out var cartao))
        {
            logger.LogError("A mensagem de pagamento no offset {Offset} não pôde ser lida.", resultado.Offset);
            return true;
        }

        var contextoAtividade = ContextoDaMensagem(resultado.Message.Headers);
        using var atividade = Telemetria.Fonte.StartActivity(
            "ConsumirPagamentoPedido",
            ActivityKind.Consumer,
            contextoAtividade);
        atividade?.SetTag("mensagem.id", mensagemId);
        atividade?.SetTag("pedido.id", pedidoId);

        await using var escopo = escopos.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<PedidosDbContext>();
        var repositorio = escopo.ServiceProvider.GetRequiredService<IPedidoRepositorio>();
        var unidadeTrabalho = escopo.ServiceProvider.GetRequiredService<IUnidadeTrabalho>();
        var gateway = escopo.ServiceProvider.GetRequiredService<IGatewayPagamento>();
        var cache = escopo.ServiceProvider.GetRequiredService<IServicoCache>();

        var jaProcessada = await contexto.MensagensProcessadas
            .AsNoTracking()
            .AnyAsync(mensagem => mensagem.MensagemId == mensagemId, cancellationToken);
        if (jaProcessada)
        {
            logger.LogInformation("A mensagem {MensagemId} já foi processada.", mensagemId);
            return true;
        }

        var pedido = await repositorio.ObterPorIdAsync(pedidoId, cancellationToken);
        if (pedido is null)
        {
            RegistrarProcessada(contexto, mensagemId);
            await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
            logger.LogWarning("O pedido {PedidoId} da mensagem {MensagemId} não existe.", pedidoId, mensagemId);
            return true;
        }

        if (pedido.Status != StatusPedido.AguardandoPagamento)
        {
            RegistrarProcessada(contexto, mensagemId);
            await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
            await cache.RemoverAsync(ChavesCache.Pedido(pedido.Id), cancellationToken);
            return true;
        }

        if (cartao is null || string.IsNullOrWhiteSpace(cartao.Token))
        {
            pedido.Cancelar("O pagamento chegou sem o token do cartão.");
            await repositorio.AtualizarAsync(pedido, cancellationToken);
            RegistrarProcessada(contexto, mensagemId);
            await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
            await cache.RemoverAsync(ChavesCache.Pedido(pedido.Id), cancellationToken);
            return true;
        }

        var autorizacao = await gateway.AutorizarAsync(
            new SolicitacaoAutorizacaoPagamento(
                pedido.Id,
                valor ?? pedido.ValorTotal.Valor,
                moeda ?? pedido.ValorTotal.Moeda,
                cartao.Token,
                cartao.Bandeira ?? string.Empty,
                cartao.EmissorId,
                cartao.Parcelas <= 0 ? 1 : cartao.Parcelas,
                cartao.EmailPagador ?? string.Empty,
                cartao.TipoDocumento ?? string.Empty,
                cartao.NumeroDocumento ?? string.Empty,
                mensagemId.ToString()),
            cancellationToken);

        if (!autorizacao.Sucesso || autorizacao.Valor is null)
        {
            logger.LogWarning(
                "O gateway não autorizou o pedido {PedidoId}: {Descricao}",
                pedido.Id,
                autorizacao.Erro?.Descricao);
            return false;
        }

        if (autorizacao.Valor.Autorizado)
            pedido.ConfirmarPagamento();
        else
            pedido.Cancelar(Motivo(autorizacao.Valor.MotivoRecusa));

        await repositorio.AtualizarAsync(pedido, cancellationToken);
        RegistrarProcessada(contexto, mensagemId);
        await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        await cache.RemoverAsync(ChavesCache.Pedido(pedido.Id), cancellationToken);
        return true;
    }

    private IConsumer<string, string> CriarConsumidor()
    {
        var configuracao = new ConsumerConfig
        {
            BootstrapServers = string.IsNullOrWhiteSpace(opcoes.Value.Servidores)
                ? ConexoesLocais.Kafka
                : opcoes.Value.Servidores,
            GroupId = Grupo,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        return new ConsumerBuilder<string, string>(configuracao).Build();
    }

    private static void RegistrarProcessada(PedidosDbContext contexto, Guid mensagemId)
    {
        contexto.MensagensProcessadas.Add(new MensagemProcessada
        {
            MensagemId = mensagemId,
            Consumidor = Nome,
            ProcessadaEmUtc = DateTime.UtcNow
        });
    }

    private static bool TentarLer(
        ConsumeResult<string, string> resultado,
        out Guid mensagemId,
        out Guid pedidoId,
        out decimal? valor,
        out string? moeda,
        out CartaoMensagem? cartao)
    {
        mensagemId = Guid.Empty;
        pedidoId = Guid.Empty;
        valor = null;
        moeda = null;
        cartao = null;

        PagamentoMensagem? corpo;
        try
        {
            corpo = JsonSerializer.Deserialize<PagamentoMensagem>(resultado.Message.Value, Json);
        }
        catch (JsonException)
        {
            return false;
        }

        if (corpo is null || corpo.PedidoId == Guid.Empty)
            return false;

        var header = LerHeader(resultado.Message.Headers, ProdutorEventosKafka.HeaderMensagemId);
        if (!Guid.TryParse(header, out mensagemId))
            mensagemId = corpo.Id;

        if (mensagemId == Guid.Empty)
            return false;

        pedidoId = corpo.PedidoId;
        valor = corpo.ValorTotal?.Valor;
        moeda = corpo.ValorTotal?.Moeda;
        cartao = corpo.Cartao;
        return true;
    }

    private static ActivityContext ContextoDaMensagem(Headers headers)
    {
        var traceParent = LerHeader(headers, ProdutorEventosKafka.HeaderTraceParent);
        if (!string.IsNullOrWhiteSpace(traceParent)
            && ActivityContext.TryParse(traceParent, null, out var contexto))
            return contexto;

        var traceId = LerHeader(headers, ProdutorEventosKafka.HeaderTraceId);
        if (string.IsNullOrWhiteSpace(traceId) || traceId.Length != 32)
            return default;

        return new ActivityContext(
            ActivityTraceId.CreateFromString(traceId),
            ActivitySpanId.CreateRandom(),
            ActivityTraceFlags.Recorded);
    }

    private static string? LerHeader(Headers headers, string nome)
    {
        var header = headers.FirstOrDefault(item => item.Key == nome);
        return header is null ? null : Encoding.UTF8.GetString(header.GetValueBytes());
    }

    private static string Motivo(string? motivoRecusa)
    {
        var motivo = string.IsNullOrWhiteSpace(motivoRecusa)
            ? "Pagamento recusado."
            : motivoRecusa.Trim();

        return motivo.Length <= 500 ? motivo : motivo[..500];
    }

    private static async Task AguardarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private sealed record PagamentoMensagem(
        Guid Id,
        Guid PedidoId,
        ValorMensagem? ValorTotal,
        CartaoMensagem? Cartao);

    private sealed record CartaoMensagem(
        string? Token,
        string? Bandeira,
        string? EmissorId,
        int Parcelas,
        string? EmailPagador,
        string? TipoDocumento,
        string? NumeroDocumento);

    private sealed record ValorMensagem(decimal Valor, string Moeda);
}
