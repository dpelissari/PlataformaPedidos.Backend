using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlataformaPedidos.Infraestrutura.Observabilidade;
using PlataformaPedidos.Infraestrutura.Persistencia;

namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Publica no Kafka as mensagens de outbox ainda não enviadas e marca a data de processamento.
/// </summary>
public sealed class PublicadorOutbox(
    IServiceScopeFactory escopos,
    ProdutorEventosKafka produtor,
    ILogger<PublicadorOutbox> logger)
{
    /// <summary>
    /// Publica um lote de mensagens pendentes. Cada publicação confirmada é gravada antes da próxima.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task PublicarPendentesAsync(CancellationToken cancellationToken)
    {
        await using var escopo = escopos.CreateAsyncScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<PedidosDbContext>();
        var pendentes = await contexto.MensagensOutbox
            .Where(mensagem => mensagem.ProcessadaEmUtc == null)
            .OrderBy(mensagem => mensagem.OcorridoEmUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var mensagem in pendentes)
        {
            await PublicarAsync(mensagem, cancellationToken);
            mensagem.ProcessadaEmUtc = DateTime.UtcNow;
            await contexto.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Mensagem {MensagemId} publicada no tópico {Topico}.",
                mensagem.Id,
                mensagem.Topico);
        }
    }

    private async Task PublicarAsync(MensagemOutbox mensagem, CancellationToken cancellationToken)
    {
        var traceParent = MontarTraceParent(mensagem.TraceId);
        ActivityContext contextoPai = default;
        if (traceParent is not null)
            ActivityContext.TryParse(traceParent, null, out contextoPai);

        using var atividade = Telemetria.Fonte.StartActivity(
            "PublicarMensagemOutbox",
            ActivityKind.Producer,
            contextoPai);
        atividade?.SetTag("mensagem.id", mensagem.Id);
        atividade?.SetTag("mensagem.topico", mensagem.Topico);

        await produtor.PublicarConteudoAsync(
            mensagem.Topico,
            Chave(mensagem),
            mensagem.Conteudo,
            mensagem.TraceId,
            traceParent,
            cancellationToken,
            mensagem.Id);
    }

    private static string? MontarTraceParent(string? traceId)
    {
        if (string.IsNullOrWhiteSpace(traceId) || traceId.Length != 32)
            return null;

        return $"00-{traceId}-{ActivitySpanId.CreateRandom()}-01";
    }

    private static string Chave(MensagemOutbox mensagem)
    {
        try
        {
            using var documento = JsonDocument.Parse(mensagem.Conteudo);
            if (documento.RootElement.TryGetProperty("pedidoId", out var pedidoId)
                && pedidoId.TryGetGuid(out var id))
                return id.ToString();
        }
        catch (JsonException)
        {
            // O conteúdo segue como está. A chave cai para o identificador da mensagem.
        }

        return mensagem.Id.ToString();
    }
}
