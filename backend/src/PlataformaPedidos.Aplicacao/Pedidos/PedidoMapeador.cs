using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Traduz o agregado de pedido para a resposta dos casos de uso.
/// </summary>
internal static class PedidoMapeador
{
    public static PedidoResposta Mapear(Pedido pedido) =>
        new(
            pedido.Id,
            pedido.ClienteId,
            pedido.EnderecoEntrega.Cep,
            pedido.EnderecoEntrega.Logradouro,
            pedido.EnderecoEntrega.Numero,
            pedido.EnderecoEntrega.Complemento,
            pedido.EnderecoEntrega.Bairro,
            pedido.EnderecoEntrega.Cidade,
            pedido.EnderecoEntrega.Estado,
            pedido.Status.ToString(),
            pedido.DataCriacaoUtc,
            pedido.ValorTotal.Valor,
            pedido.ValorTotal.Moeda,
            pedido.MotivoCancelamento,
            pedido.Itens.Select(MapearItem).ToArray());

    private static ItemPedidoResposta MapearItem(ItemPedido item) =>
        new(
            item.Id,
            item.ProdutoId,
            item.NomeProduto,
            item.Quantidade,
            item.PrecoUnitario.Valor,
            item.Subtotal.Valor);
}
