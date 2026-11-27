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
    /// URL base. No ambiente local aponta para o ViaCEP.
    /// </summary>
    public string UrlBase { get; set; } = "https://viacep.com.br";

    /// <summary>
    /// Caminho relativo. O padrão é o ViaCEP. A BrasilAPI usa api/cep/v2/{cep}.
    /// </summary>
    public string Caminho { get; set; } = "ws/{cep}/json/";
}
