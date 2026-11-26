namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Situação do ciclo de vida de um pedido.
/// </summary>
public enum StatusPedido
{
    /// <summary>
    /// Pedido aberto, ainda sem solicitação de pagamento.
    /// </summary>
    Rascunho,

    /// <summary>
    /// Pagamento solicitado e ainda não concluído.
    /// </summary>
    AguardandoPagamento,

    /// <summary>
    /// Pagamento confirmado. Estado terminal de sucesso.
    /// </summary>
    Confirmado,

    /// <summary>
    /// Pedido cancelado. Estado terminal.
    /// </summary>
    Cancelado
}
