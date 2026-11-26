using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Converte o resultado do FluentValidation no erro de aplicação.
/// </summary>
internal static class ExecutorValidacao
{
    /// <summary>
    /// Valida a entrada e devolve o erro, ou nulo quando ela é válida.
    /// </summary>
    public static async Task<ErroAplicacao?> ValidarAsync<T>(
        IValidator<T> validador,
        T instancia,
        CancellationToken cancellationToken)
    {
        var resultado = await validador.ValidateAsync(instancia, cancellationToken);
        if (resultado.IsValid)
            return null;

        var descricao = string.Join(' ', resultado.Errors.Select(erro => erro.ErrorMessage));
        return ErroAplicacao.Validacao(descricao);
    }
}
