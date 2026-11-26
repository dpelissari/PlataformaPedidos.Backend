using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Aplicacao.Pedidos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class SolicitarPagamentoPedidoManipuladorTestes
{
    private readonly IPedidoRepositorio _repositorio = Substitute.For<IPedidoRepositorio>();
    private readonly IUnidadeTrabalho _unidadeTrabalho = Substitute.For<IUnidadeTrabalho>();
    private readonly IServicoCache _cache = Substitute.For<IServicoCache>();
    private readonly SolicitarPagamentoPedidoManipulador _manipulador;

    public SolicitarPagamentoPedidoManipuladorTestes()
    {
        _manipulador = new SolicitarPagamentoPedidoManipulador(
            _repositorio,
            _unidadeTrabalho,
            _cache,
            new SolicitarPagamentoPedidoComandoValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoEmRascunho_SolicitaPagamentoEInvalidaCache()
    {
        var pedido = ApoioDominio.PedidoComItem();
        _repositorio.ObterPorIdAsync(pedido.Id, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _manipulador.ManipularAsync(
            new SolicitarPagamentoPedidoComando(pedido.Id),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Status.Should().Be("AguardandoPagamento");
        pedido.Eventos.OfType<PagamentoPedidoSolicitadoEvento>().Should().ContainSingle();
        await _repositorio.Received(1).AtualizarAsync(pedido, Arg.Any<CancellationToken>());
        await _unidadeTrabalho.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoverAsync(ChavesCache.Pedido(pedido.Id), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoInexistente_DevolveNaoEncontrado()
    {
        var pedidoId = Guid.CreateVersion7();
        _repositorio.ObterPorIdAsync(pedidoId, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var resultado = await _manipulador.ManipularAsync(
            new SolicitarPagamentoPedidoComando(pedidoId),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.NaoEncontrado);
        resultado.Erro.Codigo.Should().Be("pedido.nao_encontrado");
        await _unidadeTrabalho.DidNotReceive().SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
        await _cache.DidNotReceive().RemoverAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComComandoInvalido_DevolveValidacao()
    {
        var resultado = await _manipulador.ManipularAsync(
            new SolicitarPagamentoPedidoComando(Guid.Empty),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Validacao);
        await _repositorio.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_SemItens_DevolveConflitoSemAlterarPedido()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();
        _repositorio.ObterPorIdAsync(pedido.Id, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _manipulador.ManipularAsync(
            new SolicitarPagamentoPedidoComando(pedido.Id),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Conflito);
        pedido.Status.Should().Be(StatusPedido.Rascunho);
        await _repositorio.DidNotReceive().AtualizarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
        await _cache.DidNotReceive().RemoverAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
