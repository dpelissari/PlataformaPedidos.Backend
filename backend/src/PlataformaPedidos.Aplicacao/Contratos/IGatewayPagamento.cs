using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Dados enviados ao gateway para autorizar a cobrança de um pedido.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="Valor">Valor total cobrado.</param>
/// <param name="Moeda">Código da moeda.</param>
/// <param name="Token">Token de uso único do cartão.</param>
/// <param name="Bandeira">Bandeira do cartão.</param>
/// <param name="EmissorId">Banco emissor, quando houver.</param>
/// <param name="Parcelas">Quantidade de parcelas.</param>
/// <param name="EmailPagador">E-mail do pagador.</param>
/// <param name="TipoDocumento">Tipo do documento.</param>
/// <param name="NumeroDocumento">Número do documento.</param>
/// <param name="ChaveIdempotencia">Chave para o Mercado Pago não cobrar duas vezes a mesma mensagem.</param>
public sealed record SolicitacaoAutorizacaoPagamento(
    Guid PedidoId,
    decimal Valor,
    string Moeda,
    string Token,
    string Bandeira,
    string? EmissorId,
    int Parcelas,
    string EmailPagador,
    string TipoDocumento,
    string NumeroDocumento,
    string ChaveIdempotencia);

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
