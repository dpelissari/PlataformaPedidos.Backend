using FluentAssertions;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.TestesUnitarios;

public class PedidoTestes
{
    [Fact]
    public void Criar_ComDadosValidos_IniciaRascunhoERegistraEvento()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        pedido.Id.Should().NotBeEmpty();
        pedido.ClienteId.Should().Be(ApoioDominio.ClienteId);
        pedido.Status.Should().Be(StatusPedido.Rascunho);
        pedido.DataCriacaoUtc.Should().Be(ApoioDominio.DataFixaUtc);
        pedido.Itens.Should().BeEmpty();
        pedido.ValorTotal.Should().Be(Dinheiro.Criar(0));
        pedido.MotivoCancelamento.Should().BeNull();

        var evento = pedido.Eventos.OfType<PedidoCriadoEvento>().Should().ContainSingle().Subject;
        evento.PedidoId.Should().Be(pedido.Id);
        evento.ClienteId.Should().Be(ApoioDominio.ClienteId);
        evento.OcorridoEmUtc.Should().Be(ApoioDominio.DataFixaUtc);
        evento.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Criar_ComClienteVazio_LancaExcecaoDominio()
    {
        var acao = () => Pedido.Criar(Guid.Empty, ApoioDominio.EnderecoValido());

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void Criar_SemEndereco_LancaExcecaoDominio()
    {
        var acao = () => Pedido.Criar(ApoioDominio.ClienteId, null!);

        acao.Should().Throw<ExcecaoDominio>();
    }

    [Fact]
    public void AdicionarItem_EmRascunho_IncluiItemECalculaTotal()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();
        var outroProduto = Guid.Parse("33333333-3333-3333-3333-333333333333");

        pedido.AdicionarItem(ApoioDominio.ProdutoId, "Caderno", 2, Dinheiro.Criar(10.50m));
        pedido.AdicionarItem(outroProduto, "Caneta", 1, Dinheiro.Criar(3.25m));

        pedido.Itens.Should().HaveCount(2);
        pedido.ValorTotal.Should().Be(Dinheiro.Criar(24.25m));
    }

    [Fact]
    public void AdicionarItem_ComMesmoProdutoEPreco_AgrupaQuantidade()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        pedido.AdicionarItem(ApoioDominio.ProdutoId, "Caderno", 2, Dinheiro.Criar(10m));
        pedido.AdicionarItem(ApoioDominio.ProdutoId, "Caderno", 3, Dinheiro.Criar(10m));

        pedido.Itens.Should().ContainSingle();
        pedido.Itens[0].Quantidade.Should().Be(5);
        pedido.ValorTotal.Should().Be(Dinheiro.Criar(50m));
    }

    [Fact]
    public void AdicionarItem_ComMesmoProdutoEPrecoDiferente_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem(quantidade: 2, preco: 10m);

        var acao = () => pedido.AdicionarItem(ApoioDominio.ProdutoId, "Caderno", 1, Dinheiro.Criar(12m));

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Itens.Should().ContainSingle();
        pedido.Itens[0].Quantidade.Should().Be(2);
        pedido.ValorTotal.Should().Be(Dinheiro.Criar(20m));
    }

    [Fact]
    public void AdicionarItem_ForaDeRascunho_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();

        var acao = () => pedido.AdicionarItem(Guid.NewGuid(), "Caneta", 1, Dinheiro.Criar(2m));

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Itens.Should().ContainSingle();
        pedido.Status.Should().Be(StatusPedido.AguardandoPagamento);
    }

    [Fact]
    public void RemoverItem_EmRascunho_RemoveERecalculaTotal()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();
        var outroProduto = Guid.Parse("33333333-3333-3333-3333-333333333333");
        pedido.AdicionarItem(ApoioDominio.ProdutoId, "Caderno", 2, Dinheiro.Criar(10m));
        pedido.AdicionarItem(outroProduto, "Caneta", 1, Dinheiro.Criar(4m));
        var itemRemovido = pedido.Itens[0].Id;

        pedido.RemoverItem(itemRemovido);

        pedido.Itens.Should().ContainSingle();
        pedido.Itens[0].ProdutoId.Should().Be(outroProduto);
        pedido.ValorTotal.Should().Be(Dinheiro.Criar(4m));
    }

    [Fact]
    public void RemoverItem_Inexistente_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();

        var acao = () => pedido.RemoverItem(Guid.NewGuid());

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Itens.Should().ContainSingle();
    }

    [Fact]
    public void RemoverItem_ForaDeRascunho_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();
        var itemId = pedido.Itens[0].Id;
        pedido.SolicitarPagamento();

        var acao = () => pedido.RemoverItem(itemId);

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Itens.Should().ContainSingle();
    }

    [Fact]
    public void SolicitarPagamento_ComItens_AguardaPagamentoERegistraEvento()
    {
        var pedido = ApoioDominio.PedidoComItem();

        pedido.SolicitarPagamento();

        pedido.Status.Should().Be(StatusPedido.AguardandoPagamento);
        var evento = pedido.Eventos.OfType<PagamentoPedidoSolicitadoEvento>().Should().ContainSingle().Subject;
        evento.PedidoId.Should().Be(pedido.Id);
        evento.ValorTotal.Should().Be(pedido.ValorTotal);
        evento.ValorTotal.Should().Be(Dinheiro.Criar(21m));
    }

    [Fact]
    public void SolicitarPagamento_SemItens_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        var acao = () => pedido.SolicitarPagamento();

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Rascunho);
        pedido.Eventos.OfType<PagamentoPedidoSolicitadoEvento>().Should().BeEmpty();
    }

    [Fact]
    public void SolicitarPagamento_ForaDeRascunho_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();
        pedido.ConfirmarPagamento();

        var acao = () => pedido.SolicitarPagamento();

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Confirmado);
    }

    [Fact]
    public void ConfirmarPagamento_AguardandoPagamento_ConfirmaERegistraEvento()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();

        pedido.ConfirmarPagamento();

        pedido.Status.Should().Be(StatusPedido.Confirmado);
        pedido.Eventos.OfType<PedidoConfirmadoEvento>().Should().ContainSingle()
            .Which.PedidoId.Should().Be(pedido.Id);
    }

    [Fact]
    public void ConfirmarPagamento_EmRascunho_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();

        var acao = () => pedido.ConfirmarPagamento();

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Rascunho);
    }

    [Fact]
    public void ConfirmarPagamento_JaConfirmado_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();
        pedido.ConfirmarPagamento();

        var acao = () => pedido.ConfirmarPagamento();

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Confirmado);
        pedido.Eventos.OfType<PedidoConfirmadoEvento>().Should().ContainSingle();
    }

    [Fact]
    public void Cancelar_EmRascunho_CancelaERegistraEvento()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        pedido.Cancelar("  Cliente desistiu  ");

        pedido.Status.Should().Be(StatusPedido.Cancelado);
        pedido.MotivoCancelamento.Should().Be("Cliente desistiu");
        var evento = pedido.Eventos.OfType<PedidoCanceladoEvento>().Should().ContainSingle().Subject;
        evento.PedidoId.Should().Be(pedido.Id);
        evento.Motivo.Should().Be("Cliente desistiu");
    }

    [Fact]
    public void Cancelar_AguardandoPagamento_CancelaERegistraEvento()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();

        pedido.Cancelar("Pagamento recusado");

        pedido.Status.Should().Be(StatusPedido.Cancelado);
        pedido.MotivoCancelamento.Should().Be("Pagamento recusado");
        pedido.Eventos.OfType<PedidoCanceladoEvento>().Should().ContainSingle();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cancelar_ComMotivoVazio_LancaExcecaoDominio(string? motivo)
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        var acao = () => pedido.Cancelar(motivo!);

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Rascunho);
        pedido.MotivoCancelamento.Should().BeNull();
    }

    [Fact]
    public void Cancelar_Confirmado_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoComItem();
        pedido.SolicitarPagamento();
        pedido.ConfirmarPagamento();

        var acao = () => pedido.Cancelar("Arrependimento");

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Confirmado);
        pedido.MotivoCancelamento.Should().BeNull();
    }

    [Fact]
    public void Cancelar_JaCancelado_LancaExcecaoDominio()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();
        pedido.Cancelar("Cliente desistiu");

        var acao = () => pedido.Cancelar("Outro motivo");

        acao.Should().Throw<ExcecaoDominio>();
        pedido.Status.Should().Be(StatusPedido.Cancelado);
        pedido.MotivoCancelamento.Should().Be("Cliente desistiu");
        pedido.Eventos.OfType<PedidoCanceladoEvento>().Should().ContainSingle();
    }

    [Fact]
    public void LimparEventos_RemoveEventosRegistrados()
    {
        var pedido = ApoioDominio.PedidoEmRascunho();

        pedido.LimparEventos();

        pedido.Eventos.Should().BeEmpty();
        pedido.Status.Should().Be(StatusPedido.Rascunho);
    }
}
