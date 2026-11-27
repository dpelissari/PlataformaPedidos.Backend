using FluentValidation;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Envia o pedido para pagamento, registra o evento no agregado e invalida a consulta em cache.
/// </summary>
public sealed class SolicitarPagamentoPedidoManipulador(
    IPedidoRepositorio repositorio,
    IUnidadeTrabalho unidadeTrabalho,
    IServicoCache cache,
    IValidator<SolicitarPagamentoPedidoComando> validador)
{
    /// <summary>
    /// Executa a solicitação de pagamento.
    /// </summary>
    /// <param name="comando">Pedido que deve aguardar pagamento.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<Resultado<PedidoResposta>> ManipularAsync(
        SolicitarPagamentoPedidoComando comando,
        CancellationToken cancellationToken)
    {
        var erroValidacao = await ExecutorValidacao.ValidarAsync(validador, comando, cancellationToken);
        if (erroValidacao is not null)
            return Resultado<PedidoResposta>.Falha(erroValidacao);

        var pedido = await repositorio.ObterPorIdAsync(comando.PedidoId, cancellationToken);
        if (pedido is null)
        {
            return Resultado<PedidoResposta>.Falha(
                ErroAplicacao.NaoEncontrado("pedido.nao_encontrado", "O pedido não foi encontrado."));
        }

        try
        {
            pedido.SolicitarPagamento(new CobrancaCartao(
                comando.Token.Trim(),
                comando.Bandeira.Trim(),
                string.IsNullOrWhiteSpace(comando.EmissorId) ? null : comando.EmissorId.Trim(),
                comando.Parcelas,
                comando.EmailPagador.Trim(),
                comando.TipoDocumento.Trim(),
                new string(comando.NumeroDocumento.Where(char.IsDigit).ToArray())));
        }
        catch (ExcecaoDominio excecao)
        {
            return Resultado<PedidoResposta>.Falha(
                ErroAplicacao.Conflito("pedido.regra_violada", excecao.Message));
        }

        await repositorio.AtualizarAsync(pedido, cancellationToken);
        await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        await cache.RemoverAsync(ChavesCache.Pedido(pedido.Id), cancellationToken);
        return Resultado<PedidoResposta>.Ok(PedidoMapeador.Mapear(pedido));
    }
}
