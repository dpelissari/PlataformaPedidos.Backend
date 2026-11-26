using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Dominio.ObjetosValor;

/// <summary>
/// Endereço de entrega brasileiro, com CEP de oito dígitos e UF válida.
/// </summary>
public sealed record Endereco : ObjetoValor
{
    private static readonly HashSet<string> UnidadesFederativas = new(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    /// <summary>
    /// CEP com exatamente oito dígitos, sem máscara.
    /// </summary>
    public string Cep { get; }

    /// <summary>
    /// Logradouro da entrega.
    /// </summary>
    public string Logradouro { get; }

    /// <summary>
    /// Número do imóvel.
    /// </summary>
    public string Numero { get; }

    /// <summary>
    /// Complemento opcional, como apartamento ou bloco.
    /// </summary>
    public string? Complemento { get; }

    /// <summary>
    /// Bairro da entrega.
    /// </summary>
    public string Bairro { get; }

    /// <summary>
    /// Cidade da entrega.
    /// </summary>
    public string Cidade { get; }

    /// <summary>
    /// Sigla da unidade federativa.
    /// </summary>
    public string Estado { get; }

    private Endereco(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado)
    {
        Cep = cep;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
    }

    /// <summary>
    /// Cria um endereço válido. O CEP pode chegar com máscara e a UF em minúsculas.
    /// </summary>
    /// <param name="cep">CEP com oito dígitos numéricos.</param>
    /// <param name="logradouro">Logradouro obrigatório.</param>
    /// <param name="numero">Número obrigatório.</param>
    /// <param name="complemento">Complemento opcional.</param>
    /// <param name="bairro">Bairro obrigatório.</param>
    /// <param name="cidade">Cidade obrigatória.</param>
    /// <param name="estado">Sigla de uma UF brasileira.</param>
    /// <returns>Endereço normalizado.</returns>
    public static Endereco Criar(
        string cep,
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado)
    {
        var cepNormalizado = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());
        if (cepNormalizado.Length != 8)
            throw new ExcecaoDominio("O CEP deve conter 8 dígitos numéricos.");

        var complementoNormalizado = string.IsNullOrWhiteSpace(complemento)
            ? null
            : complemento.Trim();

        if (string.IsNullOrWhiteSpace(estado))
            throw new ExcecaoDominio("O estado é obrigatório.");

        var uf = estado.Trim().ToUpperInvariant();

        if (!UnidadesFederativas.Contains(uf))
            throw new ExcecaoDominio("O estado deve ser uma UF brasileira válida.");

        return new Endereco(
            cepNormalizado,
            ExigirTexto(logradouro, "O logradouro é obrigatório."),
            ExigirTexto(numero, "O número é obrigatório."),
            complementoNormalizado,
            ExigirTexto(bairro, "O bairro é obrigatório."),
            ExigirTexto(cidade, "A cidade é obrigatória."),
            uf);
    }

    private static string ExigirTexto(string? texto, string mensagem)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ExcecaoDominio(mensagem);

        return texto.Trim();
    }
}
