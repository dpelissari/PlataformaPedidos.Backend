using FluentValidation;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Obtém um pedido pelo identificador. A resposta fica em cache até o pedido mudar ou a chave expirar.
/// </summary>
public sealed class ObterPedidoPorIdManipulador(
    IPedidoRepositorio repositorio,
    IServicoCache cache,
    IValidator<ObterPedidoPorIdConsulta> validador)
{
    /// <summary>
    /// Executa a consulta.
    /// </summary>
    /// <param name="consulta">Identificador do pedido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<Resultado<PedidoResposta>> ManipularAsync(
        ObterPedidoPorIdConsulta consulta,
        CancellationToken cancellationToken)
    {
        var erroValidacao = await ExecutorValidacao.ValidarAsync(validador, consulta, cancellationToken);
        if (erroValidacao is not null)
            return Resultado<PedidoResposta>.Falha(erroValidacao);

        var chave = ChavesCache.Pedido(consulta.PedidoId);
        var emCache = await cache.ObterAsync<PedidoResposta>(chave, cancellationToken);
        if (emCache is not null)
            return Resultado<PedidoResposta>.Ok(emCache);

        var pedido = await repositorio.ObterPorIdAsync(consulta.PedidoId, cancellationToken);
        if (pedido is null)
        {
            return Resultado<PedidoResposta>.Falha(
                ErroAplicacao.NaoEncontrado("pedido.nao_encontrado", "O pedido não foi encontrado."));
        }

        var resposta = PedidoMapeador.Mapear(pedido);
        await cache.DefinirAsync(chave, resposta, ChavesCache.ExpiracaoPedido, cancellationToken);
        return Resultado<PedidoResposta>.Ok(resposta);
    }
}
