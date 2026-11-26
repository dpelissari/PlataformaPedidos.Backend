namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Resultado de um caso de uso que não devolve valor.
/// </summary>
public class Resultado
{
    /// <summary>
    /// Indica que o caso de uso concluiu sem erro.
    /// </summary>
    public bool Sucesso => Erro is null;

    /// <summary>
    /// Erro da operação. Nulo quando <see cref="Sucesso"/> é verdadeiro.
    /// </summary>
    public ErroAplicacao? Erro { get; }

    /// <summary>
    /// Cria o resultado com o erro informado, ou sem erro quando ele é nulo.
    /// </summary>
    /// <param name="erro">Erro da operação.</param>
    protected Resultado(ErroAplicacao? erro) => Erro = erro;

    /// <summary>
    /// Resultado concluído.
    /// </summary>
    public static Resultado Ok() => new(null);

    /// <summary>
    /// Resultado recusado.
    /// </summary>
    /// <param name="erro">Erro que explica a recusa.</param>
    public static Resultado Falha(ErroAplicacao erro)
    {
        ArgumentNullException.ThrowIfNull(erro);
        return new Resultado(erro);
    }
}

/// <summary>
/// Resultado de um caso de uso que devolve um valor quando conclui.
/// </summary>
/// <typeparam name="T">Tipo do valor devolvido.</typeparam>
public sealed class Resultado<T> : Resultado
{
    /// <summary>
    /// Valor produzido. Nulo quando a operação falha.
    /// </summary>
    public T? Valor { get; }

    private Resultado(T valor)
        : base(null) => Valor = valor;

    private Resultado(ErroAplicacao erro)
        : base(erro)
    {
    }

    /// <summary>
    /// Resultado concluído com valor.
    /// </summary>
    /// <param name="valor">Valor produzido pelo caso de uso.</param>
    public static Resultado<T> Ok(T valor) => new(valor);

    /// <summary>
    /// Resultado recusado, sem valor.
    /// </summary>
    /// <param name="erro">Erro que explica a recusa.</param>
    public static new Resultado<T> Falha(ErroAplicacao erro)
    {
        ArgumentNullException.ThrowIfNull(erro);
        return new Resultado<T>(erro);
    }
}
