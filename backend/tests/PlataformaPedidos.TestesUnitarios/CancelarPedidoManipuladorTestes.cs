using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Aplicacao.Pedidos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class CancelarPedidoManipuladorTestes
{
    private readonly IPedidoRepositorio _repositorio = Substitute.For<IPedidoRepositorio>();
    private readonly IUnidadeTrabalho _unidadeTrabalho = Substitute.For<IUnidadeTrabalho>();
    private readonly IServicoCache _cache = Substitute.For<IServicoCache>();
    private readonly CancelarPedidoManipulador _manipulador;

    public CancelarPedidoManipuladorTestes()
    {
        _manipulador = new CancelarPedidoManipulador(
            _repositorio,
            _unidadeTrabalho,
            _cache,
            new CancelarPedidoComandoValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoEmRascunho_CancelaEInvalidaCache()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();
        _repositorio.ObterPorIdAsync(pedido.Id, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _manipulador.ManipularAsync(
            new CancelarPedidoComando(pedido.Id, "  Cliente desistiu  "),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Status.Should().Be("Cancelado");
        resultado.Valor.MotivoCancelamento.Should().Be("Cliente desistiu");
        pedido.Eventos.OfType<PedidoCanceladoEvento>().Should().ContainSingle();
        await _cache.Received(1).RemoverAsync(ChavesCache.Pedido(pedido.Id), Arg.Any<CancellationToken>());
        await _unidadeTrabalho.Received(1).SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoInexistente_DevolveNaoEncontrado()
    {
        var pedidoId = Guid.CreateVersion7();
        _repositorio.ObterPorIdAsync(pedidoId, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var resultado = await _manipulador.ManipularAsync(
            new CancelarPedidoComando(pedidoId, "Cliente desistiu"),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.NaoEncontrado);
        await _repositorio.DidNotReceive().AtualizarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComMotivoVazio_DevolveValidacao()
    {
        var resultado = await _manipulador.ManipularAsync(
            new CancelarPedidoComando(Guid.CreateVersion7(), "  "),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Validacao);
        await _repositorio.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoConfirmado_DevolveConflito()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento(ApoioDominio.CobrancaCartao());
        pedido.ConfirmarPagamento();
        _repositorio.ObterPorIdAsync(pedido.Id, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _manipulador.ManipularAsync(
            new CancelarPedidoComando(pedido.Id, "Arrependimento"),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Conflito);
        pedido.Status.Should().Be(StatusPedido.Confirmado);
        await _cache.DidNotReceive().RemoverAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
