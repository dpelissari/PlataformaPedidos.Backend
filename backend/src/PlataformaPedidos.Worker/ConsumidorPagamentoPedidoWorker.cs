using PlataformaPedidos.Infraestrutura.Mensageria;

namespace PlataformaPedidos.Worker;

/// <summary>
/// Consome o pagamento solicitado e atualiza o pedido com idempotência.
/// </summary>
public sealed class ConsumidorPagamentoPedidoWorker(
    ConsumidorPagamentoPedido consumidor) : BackgroundService
{
    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        consumidor.ExecutarAsync(stoppingToken);
}
