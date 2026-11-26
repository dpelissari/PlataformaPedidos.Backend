using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Infraestrutura.Mensageria;

namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Copia os eventos dos agregados rastreados para a tabela de outbox antes do salvamento,
/// de modo que pedido e mensagem entrem na mesma transação.
/// </summary>
public sealed class InterceptadorOutbox : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConverterEventos(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConverterEventos(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ConverterEventos(DbContext? contexto)
    {
        if (contexto is null)
            return;

        var agregados = contexto.ChangeTracker
            .Entries<AgregadoRaiz>()
            .Select(entrada => entrada.Entity)
            .Where(agregado => agregado.Eventos.Count > 0)
            .ToList();

        if (agregados.Count == 0)
            return;

        var traceId = Activity.Current is { } atividade && atividade.TraceId != default
            ? atividade.TraceId.ToString()
            : null;

        foreach (var agregado in agregados)
        {
            foreach (var evento in agregado.Eventos)
            {
                contexto.Set<MensagemOutbox>().Add(new MensagemOutbox
                {
                    Id = evento.Id,
                    Tipo = evento.GetType().Name,
                    Topico = TopicosPedido.Para(evento),
                    Conteudo = JsonSerializer.Serialize(evento, evento.GetType(), Json),
                    TraceId = traceId,
                    OcorridoEmUtc = evento.OcorridoEmUtc
                });
            }

            agregado.LimparEventos();
        }
    }
}
