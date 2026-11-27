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

        RuleFor(comando => comando.Token)
            .NotEmpty()
            .WithMessage("O token do cartão é obrigatório.")
            .MaximumLength(255);

        RuleFor(comando => comando.Bandeira)
            .NotEmpty()
            .WithMessage("A bandeira do cartão é obrigatória.")
            .MaximumLength(30);

        RuleFor(comando => comando.EmissorId)
            .MaximumLength(20);

        RuleFor(comando => comando.Parcelas)
            .InclusiveBetween(1, 18)
            .WithMessage("As parcelas devem ficar entre 1 e 18.");

        RuleFor(comando => comando.EmailPagador)
            .NotEmpty()
            .WithMessage("O e-mail do pagador é obrigatório.")
            .EmailAddress()
            .WithMessage("O e-mail do pagador é inválido.")
            .MaximumLength(254);

        RuleFor(comando => comando.TipoDocumento)
            .NotEmpty()
            .WithMessage("O tipo de documento é obrigatório.")
            .MaximumLength(10);

        RuleFor(comando => comando.NumeroDocumento)
            .NotEmpty()
            .WithMessage("O número do documento é obrigatório.")
            .MaximumLength(20);
    }
}
