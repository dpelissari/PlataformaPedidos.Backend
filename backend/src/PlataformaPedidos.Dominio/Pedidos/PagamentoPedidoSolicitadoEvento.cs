using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Evento emitido quando o pedido passa a aguardar pagamento.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="ValorTotal">Valor total cobrado.</param>
/// <param name="Cartao">Token e dados do pagador usados na cobrança.</param>
/// <param name="OcorridoEmUtc">Instante UTC da solicitação.</param>
public sealed record PagamentoPedidoSolicitadoEvento(
    Guid PedidoId,
    Dinheiro ValorTotal,
    CobrancaCartao Cartao,
    DateTime OcorridoEmUtc) : IDominioEvento
{
    /// <inheritdoc />
    public Guid Id { get; } = Guid.CreateVersion7();
}
