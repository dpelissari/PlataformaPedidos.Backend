using FluentAssertions;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class ItemPedidoTestes
{
    [Fact]
    public void Criar_ComDadosValidos_CalculaSubtotal()
    {
        var item = ItemPedido.Criar(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "  Caderno  ",
            2,
            Dinheiro.Criar(10.50m));

        item.Id.Should().NotBeEmpty();
        item.NomeProduto.Should().Be("Caderno");
        item.Quantidade.Should().Be(2);
        item.Subtotal.Should().Be(Dinheiro.Criar(21m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Criar_ComQuantidadeInvalida_LancaExcecaoDominio(int quantidade)
    {
        var acao = () => ItemPedido.Criar(Guid.NewGuid(), "Caderno", quantidade, Dinheiro.Criar(10m));

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeVazio_LancaExcecaoDominio(string? nome)
    {
        var acao = () => ItemPedido.Criar(Guid.NewGuid(), nome!, 1, Dinheiro.Criar(10m));

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_ComProdutoVazio_LancaExcecaoDominio()
    {
        var acao = () => ItemPedido.Criar(Guid.Empty, "Caderno", 1, Dinheiro.Criar(10m));

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_SemPreco_LancaExcecaoDominio()
    {
        var acao = () => ItemPedido.Criar(Guid.NewGuid(), "Caderno", 1, null!);

        acao.Should().Throw<ExcecaoDominio>();
    }
}
