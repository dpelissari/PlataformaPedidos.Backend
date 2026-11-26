using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Pedidos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class ListarPedidosManipuladorTestes
{
    private readonly IPedidoRepositorio _repositorio = Substitute.For<IPedidoRepositorio>();
    private readonly ListarPedidosManipulador _manipulador;

    public ListarPedidosManipuladorTestes()
    {
        _manipulador = new ListarPedidosManipulador(_repositorio, new ListarPedidosConsultaValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComPedidosPersistidos_DevolveListaMapeada()
    {
        var rascunho = ApoioDominio.PedidoComItem();
        var cancelado = ApoioDominio.PedidoEmRascunho();
        cancelado.Cancelar("Cliente desistiu");
        _repositorio.ListarAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Pedido> { rascunho, cancelado });

        var resultado = await _manipulador.ManipularAsync(new ListarPedidosConsulta(), CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().HaveCount(2);
        resultado.Valor![0].Status.Should().Be("Rascunho");
        resultado.Valor[1].Status.Should().Be("Cancelado");
        resultado.Valor[1].MotivoCancelamento.Should().Be("Cliente desistiu");
    }

    [Fact]
    public async Task ManipularAsync_SemPedidos_DevolveListaVazia()
    {
        _repositorio.ListarAsync(Arg.Any<CancellationToken>()).Returns(Array.Empty<Pedido>());

        var resultado = await _manipulador.ManipularAsync(new ListarPedidosConsulta(), CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeEmpty();
    }
}
