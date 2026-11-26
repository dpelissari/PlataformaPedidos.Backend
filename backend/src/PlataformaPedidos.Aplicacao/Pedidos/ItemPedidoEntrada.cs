namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Item informado na criação do pedido.
/// </summary>
/// <param name="ProdutoId">Identificador do produto.</param>
/// <param name="NomeProduto">Nome do produto.</param>
/// <param name="Quantidade">Quantidade maior que zero.</param>
/// <param name="PrecoUnitario">Preço de uma unidade, com no máximo duas casas decimais.</param>
public sealed record ItemPedidoEntrada(Guid ProdutoId, string NomeProduto, int Quantidade, decimal PrecoUnitario);
