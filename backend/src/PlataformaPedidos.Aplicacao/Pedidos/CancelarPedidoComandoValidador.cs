using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Valida o comando de cancelamento.
/// </summary>
public sealed class CancelarPedidoComandoValidador : AbstractValidator<CancelarPedidoComando>
{
    /// <summary>
    /// Define as regras do comando.
    /// </summary>
    public CancelarPedidoComandoValidador()
    {
        RuleFor(comando => comando.PedidoId)
            .NotEmpty()
            .WithMessage("O pedido é obrigatório.");

        RuleFor(comando => comando.Motivo)
            .NotEmpty()
            .WithMessage("O motivo do cancelamento é obrigatório.");
    }
}
