namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Persistência do agregado de pedido. A implementação fica fora do domínio.
/// </summary>
public interface IPedidoRepositorio
{
    /// <summary>
    /// Obtém o pedido pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do pedido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O pedido encontrado ou <see langword="null"/>.</returns>
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista os pedidos persistidos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Pedidos encontrados.</returns>
    Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Inclui um pedido novo.
    /// </summary>
    /// <param name="pedido">Agregado a persistir.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza um pedido já persistido.
    /// </summary>
    /// <param name="pedido">Agregado alterado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken = default);
}
