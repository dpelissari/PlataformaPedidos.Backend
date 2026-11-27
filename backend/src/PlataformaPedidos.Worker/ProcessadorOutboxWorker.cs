using Microsoft.Extensions.Options;
using PlataformaPedidos.Infraestrutura.Mensageria;

namespace PlataformaPedidos.Worker;

/// <summary>
/// Publica a outbox no Kafka no intervalo configurado.
/// </summary>
public sealed class ProcessadorOutboxWorker(
    PublicadorOutbox publicador,
    IOptions<OpcoesOutbox> opcoes,
    ILogger<ProcessadorOutboxWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await publicador.PublicarPendentesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception excecao)
            {
                logger.LogError(excecao, "Falha ao publicar a outbox.");
            }

            try
            {
                var intervalo = opcoes.Value.Intervalo <= TimeSpan.Zero
                    ? TimeSpan.FromSeconds(1)
                    : opcoes.Value.Intervalo;
                await Task.Delay(intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
