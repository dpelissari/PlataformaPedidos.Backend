using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Valida o comando de solicitação de pagamento.
/// </summary>
public sealed class SolicitarPagamentoPedidoComandoValidador : AbstractValidator<SolicitarPagamentoPedidoComando>
{
    /// <summary>
    /// Define as regras do comando.
    /// </summary>
    public SolicitarPagamentoPedidoComandoValidador()
    {
        RuleFor(comando => comando.PedidoId)
            .NotEmpty()
            .WithMessage("O pedido é obrigatório.");
    }
}
