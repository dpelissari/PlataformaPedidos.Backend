using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Cria o contexto para o comando de migration, sem subir a API.
/// </summary>
public sealed class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    /// <inheritdoc />
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseNpgsql(ConexoesLocais.Postgres)
            .Options;

        return new PedidosDbContext(opcoes);
    }
}
