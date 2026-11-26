namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Consulta um pedido pelo identificador, usando o cache antes do repositório.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
public sealed record ObterPedidoPorIdConsulta(Guid PedidoId);
