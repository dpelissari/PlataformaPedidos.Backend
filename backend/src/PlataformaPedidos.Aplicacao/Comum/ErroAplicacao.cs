namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Erro de um caso de uso, com código estável, descrição e tipo.
/// </summary>
/// <param name="Codigo">Identificador estável do erro, usado pela borda HTTP.</param>
/// <param name="Descricao">Texto legível do problema.</param>
/// <param name="Tipo">Classificação do erro.</param>
public sealed record ErroAplicacao(string Codigo, string Descricao, TipoErroAplicacao Tipo)
{
    /// <summary>
    /// Erro de validação da entrada.
    /// </summary>
    /// <param name="descricao">Mensagens de validação reunidas.</param>
    public static ErroAplicacao Validacao(string descricao) =>
        new("validacao", descricao, TipoErroAplicacao.Validacao);

    /// <summary>
    /// Recurso inexistente.
    /// </summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="descricao">Descrição do que não foi encontrado.</param>
    public static ErroAplicacao NaoEncontrado(string codigo, string descricao) =>
        new(codigo, descricao, TipoErroAplicacao.NaoEncontrado);

    /// <summary>
    /// Operação recusada pelo estado atual.
    /// </summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="descricao">Regra que impediu a operação.</param>
    public static ErroAplicacao Conflito(string codigo, string descricao) =>
        new(codigo, descricao, TipoErroAplicacao.Conflito);

    /// <summary>
    /// Falha ao consultar um sistema externo.
    /// </summary>
    /// <param name="codigo">Código estável.</param>
    /// <param name="descricao">Descrição da falha externa.</param>
    public static ErroAplicacao FalhaExterna(string codigo, string descricao) =>
        new(codigo, descricao, TipoErroAplicacao.FalhaExterna);
}
