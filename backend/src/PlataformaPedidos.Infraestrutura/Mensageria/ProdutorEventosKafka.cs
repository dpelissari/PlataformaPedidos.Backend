using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Publica eventos de pedido no Kafka e leva o trace no header da mensagem.
/// </summary>
public sealed class ProdutorEventosKafka : IBarramentoEventos, IDisposable
{
    public const string HeaderTraceId = "TraceId";
    public const string HeaderTraceParent = "traceparent";
    public const string HeaderMensagemId = "MensagemId";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly IProducer<string, string> _produtor;

    /// <summary>
    /// Cria o produtor apontando para os brokers configurados.
    /// </summary>
    /// <param name="opcoes">Brokers do Kafka.</param>
    public ProdutorEventosKafka(IOptions<OpcoesKafka> opcoes)
    {
        var configuracao = new ProducerConfig
        {
            BootstrapServers = string.IsNullOrWhiteSpace(opcoes.Value.Servidores)
                ? ConexoesLocais.Kafka
                : opcoes.Value.Servidores,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _produtor = new ProducerBuilder<string, string>(configuracao).Build();
    }

    /// <inheritdoc />
    public Task PublicarAsync(IDominioEvento evento, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(evento);
        var traceId = Activity.Current is { } atividade && atividade.TraceId != default
            ? atividade.TraceId.ToString()
            : null;

        return PublicarConteudoAsync(
            TopicosPedido.Para(evento),
            TopicosPedido.Chave(evento),
            JsonSerializer.Serialize(evento, evento.GetType(), Json),
            traceId,
            Activity.Current?.Id,
            cancellationToken);
    }

    /// <summary>
    /// Publica o conteúdo já gravado no outbox, preservando o trace original.
    /// </summary>
    /// <param name="topico">Tópico de destino.</param>
    /// <param name="chave">Chave da mensagem, em geral o pedido.</param>
    /// <param name="conteudo">JSON do evento.</param>
    /// <param name="traceId">Trace gravado junto com o evento.</param>
    /// <param name="traceParent">Cabeçalho W3C, quando a operação ainda está ativa.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <param name="mensagemId">Identificador da mensagem de outbox, usado pelo consumidor para idempotência.</param>
    public async Task PublicarConteudoAsync(
        string topico,
        string chave,
        string conteudo,
        string? traceId,
        string? traceParent,
        CancellationToken cancellationToken = default,
        Guid? mensagemId = null)
    {
        var mensagem = new Message<string, string>
        {
            Key = chave,
            Value = conteudo,
            Headers = new Headers()
        };

        if (!string.IsNullOrWhiteSpace(traceId))
            mensagem.Headers.Add(HeaderTraceId, Encoding.UTF8.GetBytes(traceId));

        if (!string.IsNullOrWhiteSpace(traceParent))
            mensagem.Headers.Add(HeaderTraceParent, Encoding.UTF8.GetBytes(traceParent));

        if (mensagemId is Guid id && id != Guid.Empty)
            mensagem.Headers.Add(HeaderMensagemId, Encoding.UTF8.GetBytes(id.ToString()));

        await _produtor.ProduceAsync(topico, mensagem, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose() => _produtor.Dispose();
}
