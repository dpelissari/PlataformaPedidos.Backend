using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Dados enviados ao gateway para autorizar a cobrança de um pedido.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="Valor">Valor total cobrado.</param>
/// <param name="Moeda">Código da moeda.</param>
public sealed record SolicitacaoAutorizacaoPagamento(Guid PedidoId, decimal Valor, string Moeda);

/// <summary>
/// Resposta do gateway sobre a autorização da cobrança.
/// </summary>
/// <param name="Autorizado">Verdadeiro quando o pagamento foi autorizado.</param>
/// <param name="MotivoRecusa">Motivo quando a autorização é negada.</param>
public sealed record AutorizacaoPagamento(bool Autorizado, string? MotivoRecusa);

/// <summary>
/// Gateway externo que autoriza ou recusa o pagamento de um pedido.
/// </summary>
public interface IGatewayPagamento
{
    /// <summary>
    /// Solicita a autorização do pagamento. Falha de comunicação volta como falha externa.
    /// </summary>
    /// <param name="solicitacao">Cobrança a autorizar.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task<Resultado<AutorizacaoPagamento>> AutorizarAsync(
        SolicitacaoAutorizacaoPagamento solicitacao,
        CancellationToken cancellationToken = default);
}
