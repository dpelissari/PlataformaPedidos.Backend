namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Endereço do serviço de CEP. O caminho aceita o marcador {cep}.
/// </summary>
public sealed class OpcoesConsultaCep
{
    /// <summary>
    /// Nome da seção de configuração.
    /// </summary>
    public const string Secao = "ConsultaCep";

    /// <summary>
    /// URL base. No ambiente local aponta para o WireMock.
    /// </summary>
    public string UrlBase { get; set; } = ConexoesLocais.WireMock;

    /// <summary>
    /// Caminho relativo. ViaCEP usa ws/{cep}/json/ e a BrasilAPI usa api/cep/v2/{cep}.
    /// </summary>
    public string Caminho { get; set; } = "cep/{cep}";
}
