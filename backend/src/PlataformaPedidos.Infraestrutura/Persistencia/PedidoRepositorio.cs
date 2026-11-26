using Microsoft.EntityFrameworkCore;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Persistência do agregado de pedido no PostgreSQL.
/// </summary>
public sealed class PedidoRepositorio(PedidosDbContext contexto) : IPedidoRepositorio
{
    /// <inheritdoc />
    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        contexto.Pedidos
            .Include(pedido => pedido.Itens)
            .FirstOrDefaultAsync(pedido => pedido.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var pedidos = await contexto.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Itens)
            .OrderByDescending(pedido => pedido.DataCriacaoUtc)
            .ToListAsync(cancellationToken);

        return pedidos;
    }

    /// <inheritdoc />
    public Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        contexto.Pedidos.Add(pedido);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        contexto.Pedidos.Update(pedido);
        return Task.CompletedTask;
    }
}
