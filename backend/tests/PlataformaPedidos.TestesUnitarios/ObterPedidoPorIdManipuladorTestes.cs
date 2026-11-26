using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Aplicacao.Pedidos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class ObterPedidoPorIdManipuladorTestes
{
    private readonly IPedidoRepositorio _repositorio = Substitute.For<IPedidoRepositorio>();
    private readonly IServicoCache _cache = Substitute.For<IServicoCache>();
    private readonly ObterPedidoPorIdManipulador _manipulador;

    public ObterPedidoPorIdManipuladorTestes()
    {
        _manipulador = new ObterPedidoPorIdManipulador(
            _repositorio,
            _cache,
            new ObterPedidoPorIdConsultaValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComCacheHit_DevolveRespostaSemConsultarRepositorio()
    {
        var resposta = RespostaEmCache();
        _cache.ObterAsync<PedidoResposta>(ChavesCache.Pedido(resposta.Id), Arg.Any<CancellationToken>())
            .Returns(resposta);

        var resultado = await _manipulador.ManipularAsync(
            new ObterPedidoPorIdConsulta(resposta.Id),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeSameAs(resposta);
        await _repositorio.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _cache.DidNotReceive().DefinirAsync(
            Arg.Any<string>(),
            Arg.Any<PedidoResposta>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComCacheMiss_ConsultaRepositorioEGravaCache()
    {
        var pedido = ApoioDominio.PedidoComItem();
        _repositorio.ObterPorIdAsync(pedido.Id, Arg.Any<CancellationToken>()).Returns(pedido);

        var resultado = await _manipulador.ManipularAsync(
            new ObterPedidoPorIdConsulta(pedido.Id),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Id.Should().Be(pedido.Id);
        resultado.Valor.ValorTotal.Should().Be(21m);
        resultado.Valor.Status.Should().Be("Rascunho");
        await _cache.Received(1).DefinirAsync(
            ChavesCache.Pedido(pedido.Id),
            Arg.Is<PedidoResposta>(resposta => resposta.Id == pedido.Id),
            ChavesCache.ExpiracaoPedido,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidoInexistente_DevolveNaoEncontrado()
    {
        var pedidoId = Guid.CreateVersion7();
        _repositorio.ObterPorIdAsync(pedidoId, Arg.Any<CancellationToken>()).Returns((Pedido?)null);

        var resultado = await _manipulador.ManipularAsync(
            new ObterPedidoPorIdConsulta(pedidoId),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.NaoEncontrado);
        await _cache.DidNotReceive().DefinirAsync(
            Arg.Any<string>(),
            Arg.Any<PedidoResposta>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComConsultaInvalida_DevolveValidacao()
    {
        var resultado = await _manipulador.ManipularAsync(
            new ObterPedidoPorIdConsulta(Guid.Empty),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Validacao);
        await _cache.DidNotReceive().ObterAsync<PedidoResposta>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _repositorio.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    private static PedidoResposta RespostaEmCache() =>
        new(
            Guid.CreateVersion7(),
            ApoioDominio.ClienteId,
            "01310100",
            "Avenida Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP",
            "Rascunho",
            ApoioDominio.DataFixaUtc,
            21m,
            "BRL",
            null,
            []);
}
