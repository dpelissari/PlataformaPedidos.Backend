using FluentValidation;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Lista os pedidos persistidos.
/// </summary>
public sealed class ListarPedidosManipulador(
    IPedidoRepositorio repositorio,
    IValidator<ListarPedidosConsulta> validador)
{
    /// <summary>
    /// Executa a listagem.
    /// </summary>
    /// <param name="consulta">Consulta sem filtro.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<Resultado<IReadOnlyList<PedidoResposta>>> ManipularAsync(
        ListarPedidosConsulta consulta,
        CancellationToken cancellationToken)
    {
        var erroValidacao = await ExecutorValidacao.ValidarAsync(validador, consulta, cancellationToken);
        if (erroValidacao is not null)
            return Resultado<IReadOnlyList<PedidoResposta>>.Falha(erroValidacao);

        var pedidos = await repositorio.ListarAsync(cancellationToken);
        var resposta = pedidos.Select(PedidoMapeador.Mapear).ToArray();
        return Resultado<IReadOnlyList<PedidoResposta>>.Ok(resposta);
    }
}
