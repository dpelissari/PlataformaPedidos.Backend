namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Comando para criar um pedido a partir do CEP, do número e dos itens.
/// </summary>
/// <param name="ClienteId">Identificador do cliente.</param>
/// <param name="Cep">CEP da entrega, com ou sem máscara.</param>
/// <param name="Numero">Número da entrega.</param>
/// <param name="Complemento">Complemento opcional.</param>
/// <param name="Itens">Itens do pedido.</param>
public sealed record CriarPedidoComando(
    Guid ClienteId,
    string Cep,
    string Numero,
    string? Complemento,
    IReadOnlyList<ItemPedidoEntrada> Itens);
