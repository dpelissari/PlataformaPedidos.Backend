using FluentValidation;
using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Valida o comando de criação do pedido.
/// </summary>
public sealed class CriarPedidoComandoValidador : AbstractValidator<CriarPedidoComando>
{
    /// <summary>
    /// Define as regras do comando.
    /// </summary>
    public CriarPedidoComandoValidador()
    {
        RuleFor(comando => comando.ClienteId)
            .NotEmpty()
            .WithMessage("O cliente do pedido é obrigatório.");

        RuleFor(comando => comando.Cep).CepObrigatorio();

        RuleFor(comando => comando.Numero)
            .NotEmpty()
            .WithMessage("O número é obrigatório.");

        RuleFor(comando => comando.Itens)
            .NotNull()
            .WithMessage("Os itens do pedido são obrigatórios.");

        RuleFor(comando => comando.Itens)
            .NotEmpty()
            .When(comando => comando.Itens is not null)
            .WithMessage("O pedido precisa ter ao menos um item.");

        RuleForEach(comando => comando.Itens)
            .SetValidator(new ItemPedidoEntradaValidador())
            .When(comando => comando.Itens is not null);
    }
}
