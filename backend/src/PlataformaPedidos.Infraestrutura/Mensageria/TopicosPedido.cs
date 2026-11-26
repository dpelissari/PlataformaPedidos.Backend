using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Nomes dos tópicos em que os eventos de pedido são publicados.
/// </summary>
public static class TopicosPedido
{
    /// <summary>
    /// Pedido criado em rascunho.
    /// </summary>
    public const string Criado = "pedidos.criado";

    /// <summary>
    /// Pagamento solicitado e ainda não concluído.
    /// </summary>
    public const string PagamentoSolicitado = "pedidos.pagamento-solicitado";

    /// <summary>
    /// Pagamento confirmado.
    /// </summary>
    public const string Confirmado = "pedidos.confirmado";

    /// <summary>
    /// Pedido cancelado.
    /// </summary>
    public const string Cancelado = "pedidos.cancelado";

    /// <summary>
    /// Resolve o tópico do evento.
    /// </summary>
    /// <param name="evento">Evento de domínio do pedido.</param>
    public static string Para(IDominioEvento evento) => evento switch
    {
        PedidoCriadoEvento => Criado,
        PagamentoPedidoSolicitadoEvento => PagamentoSolicitado,
        PedidoConfirmadoEvento => Confirmado,
        PedidoCanceladoEvento => Cancelado,
        _ => throw new InvalidOperationException($"O evento {evento.GetType().Name} não tem tópico.")
    };

    /// <summary>
    /// Usa o pedido como chave da mensagem, para manter a ordem dele na partição.
    /// </summary>
    /// <param name="evento">Evento de domínio do pedido.</param>
    public static string Chave(IDominioEvento evento) => evento switch
    {
        PedidoCriadoEvento criado => criado.PedidoId.ToString(),
        PagamentoPedidoSolicitadoEvento pagamento => pagamento.PedidoId.ToString(),
        PedidoConfirmadoEvento confirmado => confirmado.PedidoId.ToString(),
        PedidoCanceladoEvento cancelado => cancelado.PedidoId.ToString(),
        _ => evento.Id.ToString()
    };
}
