using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Evento emitido quando o pagamento do pedido é confirmado.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="OcorridoEmUtc">Instante UTC da confirmação.</param>
public sealed record PedidoConfirmadoEvento(
    Guid PedidoId,
    DateTime OcorridoEmUtc) : IDominioEvento
{
    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();
}
