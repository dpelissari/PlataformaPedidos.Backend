using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Dominio.ObjetosValor;

/// <summary>
/// Valor monetário imutável, não negativo e com no máximo duas casas decimais.
/// </summary>
public sealed record Dinheiro : ObjetoValor
{
    /// <summary>
    /// Quantia na moeda informada.
    /// </summary>
    public decimal Valor { get; }

    /// <summary>
    /// Código da moeda em letras maiúsculas. O padrão é BRL.
    /// </summary>
    public string Moeda { get; }

    private Dinheiro(decimal valor, string moeda)
    {
        Valor = valor;
        Moeda = moeda;
    }

    /// <summary>
    /// Cria um valor monetário válido.
    /// </summary>
    /// <param name="valor">Quantia maior ou igual a zero, com no máximo duas casas decimais.</param>
    /// <param name="moeda">Código da moeda. Quando omitido, usa BRL.</param>
    /// <returns>Nova instância imutável.</returns>
    public static Dinheiro Criar(decimal valor, string moeda = "BRL")
    {
        if (valor < 0)
            throw new ExcecaoDominio("O valor monetário não pode ser negativo.");

        if (decimal.Round(valor, 2, MidpointRounding.AwayFromZero) != valor)
            throw new ExcecaoDominio("O valor monetário aceita no máximo duas casas decimais.");

        if (string.IsNullOrWhiteSpace(moeda))
            throw new ExcecaoDominio("A moeda é obrigatória.");

        return new Dinheiro(valor, moeda.Trim().ToUpperInvariant());
    }

    /// <summary>
    /// Soma outro valor da mesma moeda e devolve uma nova instância.
    /// </summary>
    /// <param name="outro">Valor somado.</param>
    /// <returns>Soma imutável.</returns>
    public Dinheiro Somar(Dinheiro outro)
    {
        if (outro is null)
            throw new ExcecaoDominio("O valor somado é obrigatório.");

        if (!string.Equals(Moeda, outro.Moeda, StringComparison.Ordinal))
            throw new ExcecaoDominio("Não é possível somar valores de moedas diferentes.");

        return Criar(Valor + outro.Valor, Moeda);
    }

    /// <summary>
    /// Multiplica a quantia por um fator inteiro não negativo e devolve uma nova instância arredondada para duas casas.
    /// </summary>
    /// <param name="fator">Quantidade de vezes. Zero é aceito.</param>
    /// <returns>Produto imutável.</returns>
    public Dinheiro Multiplicar(int fator)
    {
        if (fator < 0)
            throw new ExcecaoDominio("O fator de multiplicação não pode ser negativo.");

        var produto = decimal.Round(Valor * fator, 2, MidpointRounding.AwayFromZero);
        return Criar(produto, Moeda);
    }
}
