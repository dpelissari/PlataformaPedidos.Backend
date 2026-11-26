using FluentValidation;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Cria o pedido depois de resolver o endereço pelo CEP e persiste o agregado.
/// </summary>
public sealed class CriarPedidoManipulador(
    IPedidoRepositorio repositorio,
    IUnidadeTrabalho unidadeTrabalho,
    IServicoConsultaCep servicoConsultaCep,
    IValidator<CriarPedidoComando> validador)
{
    /// <summary>
    /// Executa a criação do pedido.
    /// </summary>
    /// <param name="comando">Dados do pedido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<Resultado<PedidoResposta>> ManipularAsync(
        CriarPedidoComando comando,
        CancellationToken cancellationToken)
    {
        var erroValidacao = await ExecutorValidacao.ValidarAsync(validador, comando, cancellationToken);
        if (erroValidacao is not null)
            return Resultado<PedidoResposta>.Falha(erroValidacao);

        var consultaCep = await servicoConsultaCep.ConsultarAsync(
            CepNormalizador.Normalizar(comando.Cep),
            cancellationToken);

        if (!consultaCep.Sucesso || consultaCep.Valor is null)
        {
            return Resultado<PedidoResposta>.Falha(
                consultaCep.Erro
                ?? ErroAplicacao.FalhaExterna("cep.indisponivel", "A consulta de CEP não retornou endereço."));
        }

        Pedido pedido;
        try
        {
            pedido = MontarPedido(comando, consultaCep.Valor);
        }
        catch (ExcecaoDominio excecao)
        {
            return Resultado<PedidoResposta>.Falha(
                ErroAplicacao.Conflito("pedido.regra_violada", excecao.Message));
        }

        await repositorio.AdicionarAsync(pedido, cancellationToken);
        await unidadeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        return Resultado<PedidoResposta>.Ok(PedidoMapeador.Mapear(pedido));
    }

    private static Pedido MontarPedido(CriarPedidoComando comando, EnderecoExterno enderecoExterno)
    {
        var endereco = Endereco.Criar(
            enderecoExterno.Cep,
            enderecoExterno.Logradouro,
            comando.Numero,
            comando.Complemento,
            enderecoExterno.Bairro,
            enderecoExterno.Cidade,
            enderecoExterno.Estado);

        var pedido = Pedido.Criar(comando.ClienteId, endereco);
        foreach (var item in comando.Itens)
        {
            pedido.AdicionarItem(
                item.ProdutoId,
                item.NomeProduto,
                item.Quantidade,
                Dinheiro.Criar(item.PrecoUnitario));
        }

        return pedido;
    }
}
