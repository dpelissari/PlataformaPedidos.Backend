namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Pedido e token de cartão para iniciar a cobrança.
/// </summary>
/// <param name="PedidoId">Identificador do pedido.</param>
/// <param name="Token">Token de uso único gerado no navegador.</param>
/// <param name="Bandeira">Bandeira informada pelo Mercado Pago.</param>
/// <param name="EmissorId">Banco emissor, quando houver.</param>
/// <param name="Parcelas">Quantidade de parcelas.</param>
/// <param name="EmailPagador">E-mail do pagador.</param>
/// <param name="TipoDocumento">Tipo do documento.</param>
/// <param name="NumeroDocumento">Número do documento.</param>
public sealed record SolicitarPagamentoPedidoComando(
    Guid PedidoId,
    string Token,
    string Bandeira,
    string? EmissorId,
    int Parcelas,
    string EmailPagador,
    string TipoDocumento,
    string NumeroDocumento);
