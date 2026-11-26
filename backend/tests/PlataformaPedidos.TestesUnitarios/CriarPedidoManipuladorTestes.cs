using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Aplicacao.Pedidos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class CriarPedidoManipuladorTestes
{
    private readonly IPedidoRepositorio _repositorio = Substitute.For<IPedidoRepositorio>();
    private readonly IUnidadeTrabalho _unidadeTrabalho = Substitute.For<IUnidadeTrabalho>();
    private readonly IServicoConsultaCep _consultaCep = Substitute.For<IServicoConsultaCep>();
    private readonly CriarPedidoManipulador _manipulador;

    public CriarPedidoManipuladorTestes()
    {
        _manipulador = new CriarPedidoManipulador(
            _repositorio,
            _unidadeTrabalho,
            _consultaCep,
            new CriarPedidoComandoValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComDadosValidos_CriaPedidoEPersiste()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Ok(EnderecoExternoValido()));

        var resultado = await _manipulador.ManipularAsync(ComandoValido(), CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Status.Should().Be("Rascunho");
        resultado.Valor.Logradouro.Should().Be("Avenida Paulista");
        resultado.Valor.Numero.Should().Be("1000");
        resultado.Valor.Complemento.Should().Be("apto 42");
        resultado.Valor.ValorTotal.Should().Be(21m);
        resultado.Valor.Moeda.Should().Be("BRL");
        await _repositorio.Received(1).AdicionarAsync(
            Arg.Is<Pedido>(pedido =>
                pedido.ClienteId == ApoioDominio.ClienteId
                && pedido.Itens.Count == 1
                && pedido.Eventos.OfType<PedidoCriadoEvento>().Any()),
            Arg.Any<CancellationToken>());
        await _unidadeTrabalho.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComComandoInvalido_DevolveValidacaoSemConsultarCep()
    {
        var comando = ComandoValido() with { ClienteId = Guid.Empty };

        var resultado = await _manipulador.ManipularAsync(comando, CancellationToken.None);

        resultado.Sucesso.Should().BeFalse();
        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Validacao);
        await _consultaCep.DidNotReceive().ConsultarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComCepInexistente_DevolveNaoEncontrado()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Falha(
                ErroAplicacao.NaoEncontrado("cep.nao_encontrado", "CEP não encontrado.")));

        var resultado = await _manipulador.ManipularAsync(ComandoValido(), CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.NaoEncontrado);
        resultado.Erro.Codigo.Should().Be("cep.nao_encontrado");
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComFalhaExternaDeCep_DevolveFalhaExterna()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Falha(
                ErroAplicacao.FalhaExterna("cep.indisponivel", "Serviço de CEP indisponível.")));

        var resultado = await _manipulador.ManipularAsync(ComandoValido(), CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.FalhaExterna);
        await _unidadeTrabalho.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComMesmoProdutoEPrecosDiferentes_DevolveConflitoSemPersistir()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Ok(EnderecoExternoValido()));
        var comando = ComandoValido() with
        {
            Itens =
            [
                new ItemPedidoEntrada(ApoioDominio.ProdutoId, "Caderno", 1, 10m),
                new ItemPedidoEntrada(ApoioDominio.ProdutoId, "Caderno", 1, 12m)
            ]
        };

        var resultado = await _manipulador.ManipularAsync(comando, CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Conflito);
        await _repositorio.DidNotReceive().AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
    }

    private static CriarPedidoComando ComandoValido() =>
        new(
            ApoioDominio.ClienteId,
            "01310-100",
            "1000",
            "apto 42",
            [new ItemPedidoEntrada(ApoioDominio.ProdutoId, "Caderno", 2, 10.50m)]);

    private static EnderecoExterno EnderecoExternoValido() =>
        new("01310100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");
}
