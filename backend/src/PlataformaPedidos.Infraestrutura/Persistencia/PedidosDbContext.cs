using Microsoft.EntityFrameworkCore;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Contexto do PostgreSQL para pedidos, outbox e idempotência dos consumidores.
/// </summary>
public sealed class PedidosDbContext(DbContextOptions<PedidosDbContext> opcoes) : DbContext(opcoes)
{
    /// <summary>
    /// Pedidos persistidos.
    /// </summary>
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    /// <summary>
    /// Eventos gravados na mesma transação do pedido e ainda não publicados, ou já publicados.
    /// </summary>
    public DbSet<MensagemOutbox> MensagensOutbox => Set<MensagemOutbox>();

    /// <summary>
    /// Mensagens que um consumidor já tratou.
    /// </summary>
    public DbSet<MensagemProcessada> MensagensProcessadas => Set<MensagemProcessada>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
    }
}
