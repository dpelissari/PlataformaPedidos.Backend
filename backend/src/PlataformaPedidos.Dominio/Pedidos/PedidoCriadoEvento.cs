using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Evento emitido quando um pedido nasce em rascunho.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="ClienteId">Identificador do cliente.</param>
/// <param name="OcorridoEmUtc">Instante UTC da criação.</param>
public sealed record PedidoCriadoEvento(
    Guid PedidoId,
    Guid ClienteId,
    DateTime OcorridoEmUtc) : IDominioEvento
{
    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();
}
