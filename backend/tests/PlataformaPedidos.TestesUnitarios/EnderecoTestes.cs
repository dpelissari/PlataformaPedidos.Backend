using FluentAssertions;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;

namespace PlataformaPedidos.TestesUnitarios;

public class EnderecoTestes
{
    [Fact]
    public void Criar_ComCepMascarado_NormalizaCampos()
    {
        var endereco = Endereco.Criar(
            "01310-100",
            " Avenida Paulista ",
            " 1000 ",
            "  apto 42 ",
            " Bela Vista ",
            " São Paulo ",
            "sp");

        endereco.Cep.Should().Be("01310100");
        endereco.Logradouro.Should().Be("Avenida Paulista");
        endereco.Numero.Should().Be("1000");
        endereco.Complemento.Should().Be("apto 42");
        endereco.Bairro.Should().Be("Bela Vista");
        endereco.Cidade.Should().Be("São Paulo");
        endereco.Estado.Should().Be("SP");
    }

    [Fact]
    public void Criar_ComComplementoVazio_DefineComplementoNulo()
    {
        var endereco = Endereco.Criar("01310100", "Rua A", "10", "   ", "Centro", "Campinas", "SP");

        endereco.Complemento.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("ABCDEFGH")]
    public void Criar_ComCepInvalido_LancaExcecaoDominio(string cep)
    {
        var acao = () => Endereco.Criar(cep, "Rua A", "10", null, "Centro", "Campinas", "SP");

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Theory]
    [InlineData("logradouro")]
    [InlineData("numero")]
    [InlineData("bairro")]
    [InlineData("cidade")]
    [InlineData("estado")]
    public void Criar_ComCampoObrigatorioVazio_LancaExcecaoDominio(string campo)
    {
        var acao = () => Endereco.Criar(
            "01310100",
            campo == "logradouro" ? " " : "Rua A",
            campo == "numero" ? " " : "10",
            null,
            campo == "bairro" ? " " : "Centro",
            campo == "cidade" ? " " : "Campinas",
            campo == "estado" ? " " : "SP");

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("São Paulo")]
    [InlineData("S")]
    public void Criar_ComUfInvalida_LancaExcecaoDominio(string estado)
    {
        var acao = () => Endereco.Criar("01310100", "Rua A", "10", null, "Centro", "Campinas", estado);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_ComMesmosValores_SaoIguais()
    {
        var primeiro = Endereco.Criar("01310-100", "Rua A", "10", null, "Centro", "Campinas", "sp");
        var segundo = Endereco.Criar("01310100", "Rua A", "10", null, "Centro", "Campinas", "SP");

        primeiro.Should().Be(segundo);
    }
}
