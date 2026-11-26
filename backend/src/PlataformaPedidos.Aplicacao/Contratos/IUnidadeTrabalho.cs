namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Confirma a persistência das alterações acumuladas na mesma transação.
/// </summary>
public interface IUnidadeTrabalho
{
    /// <summary>
    /// Grava as alterações pendentes.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
