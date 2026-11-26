namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Comando para cancelar um pedido que ainda não foi confirmado.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="Motivo">Motivo obrigatório do cancelamento.</param>
public sealed record CancelarPedidoComando(Guid PedidoId, string Motivo);
