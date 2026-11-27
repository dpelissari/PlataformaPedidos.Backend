namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Cartão já convertido em token pelo Mercado Pago. O número não entra no domínio.
/// </summary>
/// <param name="Token">Token de uso único gerado no navegador.</param>
/// <param name="Bandeira">Identificador da bandeira, como visa ou master.</param>
/// <param name="EmissorId">Banco emissor, quando o Mercado Pago informar.</param>
/// <param name="Parcelas">Quantidade de parcelas, no mínimo uma.</param>
/// <param name="EmailPagador">E-mail do pagador.</param>
/// <param name="TipoDocumento">Tipo do documento, como CPF.</param>
/// <param name="NumeroDocumento">Número do documento, só dígitos.</param>
public sealed record CobrancaCartao(
    string Token,
    string Bandeira,
    string? EmissorId,
    int Parcelas,
    string EmailPagador,
    string TipoDocumento,
    string NumeroDocumento);
