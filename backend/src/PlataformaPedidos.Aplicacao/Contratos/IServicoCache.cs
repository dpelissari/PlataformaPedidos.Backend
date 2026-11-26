namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Cache distribuído usado pelas consultas para evitar ida repetida ao banco ou a um serviço externo.
/// </summary>
public interface IServicoCache
{
    /// <summary>
    /// Obtém o valor associado à chave.
    /// </summary>
    /// <typeparam name="T">Tipo do valor armazenado.</typeparam>
    /// <param name="chave">Chave do cache.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O valor encontrado ou nulo quando a chave não existe.</returns>
    Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Grava o valor com tempo de expiração.
    /// </summary>
    /// <typeparam name="T">Tipo do valor armazenado.</typeparam>
    /// <param name="chave">Chave do cache.</param>
    /// <param name="valor">Valor a armazenar.</param>
    /// <param name="expiracao">Tempo até a chave expirar.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Remove a chave, invalidando a consulta associada.
    /// </summary>
    /// <param name="chave">Chave do cache.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task RemoverAsync(string chave, CancellationToken cancellationToken = default);
}
