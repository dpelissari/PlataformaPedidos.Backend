using PlataformaPedidos.Aplicacao.Contratos;

namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Confirma no banco as alterações acumuladas, incluindo as mensagens de outbox.
/// </summary>
public sealed class UnidadeTrabalho(PedidosDbContext contexto) : IUnidadeTrabalho
{
    /// <inheritdoc />
    public Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default) =>
        contexto.SaveChangesAsync(cancellationToken);
}
