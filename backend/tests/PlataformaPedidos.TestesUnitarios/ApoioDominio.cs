using PlataformaPedidos.Dominio.ObjetosValor;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

internal static class ApoioDominio
{
    public static readonly Guid ClienteId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ProdutoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly DateTime DataFixaUtc = new(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc);

    public static Endereco EnderecoValido() =>
        Endereco.Criar("01310100", "Avenida Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");

    public static Pedido PedidoEmRascunho() =>
        Pedido.Criar(ClienteId, EnderecoValido(), DataFixaUtc);

    public static Pedido PedidoComItem(int quantidade = 2, decimal preco = 10.50m)
    {
        var pedido = PedidoEmRascunho();
        pedido.AdicionarItem(ProdutoId, "Caderno", quantidade, Dinheiro.Criar(preco));
        return pedido;
    }
}
