namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Endereço do gateway de pagamento simulado pelo WireMock.
/// </summary>
public sealed class OpcoesGatewayPagamento
{
    /// <summary>
    /// Nome da seção de configuração.
    /// </summary>
    public const string Secao = "GatewayPagamento";

    /// <summary>
    /// URL base. No ambiente local aponta para o WireMock.
    /// </summary>
    public string UrlBase { get; set; } = ConexoesLocais.WireMock;

    /// <summary>
    /// Caminho relativo da autorização.
    /// </summary>
    public string Caminho { get; set; } = "pagamentos/autorizar";
}
