using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Evento emitido quando o pedido é cancelado.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="Motivo">Motivo informado no cancelamento.</param>
/// <param name="OcorridoEmUtc">Instante UTC do cancelamento.</param>
public sealed record PedidoCanceladoEvento(
    Guid PedidoId,
    string Motivo,
    DateTime OcorridoEmUtc) : IDominioEvento
{
    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();
}
