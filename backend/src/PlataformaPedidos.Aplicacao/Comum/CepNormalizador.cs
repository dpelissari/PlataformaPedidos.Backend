namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Normaliza um CEP para oito dígitos, descartando a máscara.
/// </summary>
public static class CepNormalizador
{
    /// <summary>
    /// Extrai só os dígitos do CEP.
    /// </summary>
    /// <param name="cep">CEP informado, com ou sem máscara.</param>
    /// <returns>Dígitos encontrados.</returns>
    public static string Normalizar(string? cep) =>
        new((cep ?? string.Empty).Where(char.IsDigit).ToArray());

    /// <summary>
    /// Indica se o CEP tem exatamente oito dígitos depois da normalização.
    /// </summary>
    /// <param name="cep">CEP informado.</param>
    public static bool TemOitoDigitos(string? cep) => Normalizar(cep).Length == 8;
}
