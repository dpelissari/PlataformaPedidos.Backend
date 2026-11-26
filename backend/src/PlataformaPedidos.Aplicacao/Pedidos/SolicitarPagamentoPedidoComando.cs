namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Comando para solicitar o pagamento de um pedido em rascunho.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
public sealed record SolicitarPagamentoPedidoComando(Guid PedidoId);
