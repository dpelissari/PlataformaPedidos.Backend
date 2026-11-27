namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Credenciais e endereço da API de pagamentos do Mercado Pago.
/// O token de acesso não vai para o repositório.
/// </summary>
public sealed class OpcoesMercadoPago
{
    /// <summary>
    /// Nome da seção de configuração.
    /// </summary>
    public const string Secao = "MercadoPago";

    /// <summary>
    /// URL base da API. Nos testes aponta para o WireMock.
    /// </summary>
    public string UrlBase { get; set; } = "https://api.mercadopago.com";

    /// <summary>
    /// Caminho relativo da criação do pagamento.
    /// </summary>
    public string Caminho { get; set; } = "v1/orders";

    /// <summary>
    /// Access Token de teste ou de produção. Fica vazio no repositório.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Public Key usada pelo navegador para gerar o token do cartão.
    /// </summary>
    public string ChavePublica { get; set; } = string.Empty;
}
