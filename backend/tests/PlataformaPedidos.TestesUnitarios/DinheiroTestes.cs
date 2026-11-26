using FluentAssertions;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;

namespace PlataformaPedidos.TestesUnitarios;

public class DinheiroTestes
{
    [Fact]
    public void Criar_ComValorPositivo_DefineValorEMoedaPadrao()
    {
        var dinheiro = Dinheiro.Criar(10.50m);

        dinheiro.Valor.Should().Be(10.50m);
        dinheiro.Moeda.Should().Be("BRL");
    }

    [Fact]
    public void Criar_ComValorZero_Aceita()
    {
        var dinheiro = Dinheiro.Criar(0m);

        dinheiro.Valor.Should().Be(0m);
    }

    [Fact]
    public void Criar_ComValorNegativo_LancaExcecaoDominio()
    {
        var acao = () => Dinheiro.Criar(-0.01m);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_ComMaisDeDuasCasas_LancaExcecaoDominio()
    {
        var acao = () => Dinheiro.Criar(1.234m);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_ComMoedaInformada_NormalizaParaMaiusculas()
    {
        var dinheiro = Dinheiro.Criar(5m, " usd ");

        dinheiro.Moeda.Should().Be("USD");
        dinheiro.Valor.Should().Be(5m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComMoedaVazia_LancaExcecaoDominio(string? moeda)
    {
        var acao = () => Dinheiro.Criar(1m, moeda!);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_ComMesmoValorEMoeda_SaoIguais()
    {
        var primeiro = Dinheiro.Criar(10m, "brl");
        var segundo = Dinheiro.Criar(10m);

        primeiro.Should().Be(segundo);
    }

    [Fact]
    public void Somar_ComMesmaMoeda_DevolveNovoValorSemAlterarOrigem()
    {
        var origem = Dinheiro.Criar(10.50m);
        var outro = Dinheiro.Criar(2.25m);

        var soma = origem.Somar(outro);

        soma.Valor.Should().Be(12.75m);
        soma.Moeda.Should().Be("BRL");
        origem.Valor.Should().Be(10.50m);
        outro.Valor.Should().Be(2.25m);
    }

    [Fact]
    public void Somar_ComMoedasDiferentes_LancaExcecaoDominio()
    {
        var real = Dinheiro.Criar(10m);
        var dolar = Dinheiro.Criar(10m, "USD");

        var acao = () => real.Somar(dolar);

        acao.Should().Throw<ExcecaoDominio>();
        real.Valor.Should().Be(10m);
    }

    [Fact]
    public void Somar_SemOutroValor_LancaExcecaoDominio()
    {
        var acao = () => Dinheiro.Criar(1m).Somar(null!);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Multiplicar_ComFatorPositivo_DevolveNovoValorSemAlterarOrigem()
    {
        var origem = Dinheiro.Criar(10.55m);

        var produto = origem.Multiplicar(3);

        produto.Valor.Should().Be(31.65m);
        produto.Moeda.Should().Be("BRL");
        origem.Valor.Should().Be(10.55m);
    }

    [Fact]
    public void Multiplicar_ComFatorZero_DevolveZero()
    {
        var origem = Dinheiro.Criar(10m);

        var produto = origem.Multiplicar(0);

        produto.Valor.Should().Be(0m);
        origem.Valor.Should().Be(10m);
    }

    [Fact]
    public void Multiplicar_ComFatorNegativo_LancaExcecaoDominio()
    {
        var origem = Dinheiro.Criar(10m);

        var acao = () => origem.Multiplicar(-1);

        acao.Should().Throw<ExcecaoDominio>();
        origem.Valor.Should().Be(10m);
    }
}
