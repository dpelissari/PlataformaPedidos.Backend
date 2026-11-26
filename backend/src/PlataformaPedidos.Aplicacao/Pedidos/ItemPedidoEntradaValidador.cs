using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Valida cada item informado na criação do pedido.
/// </summary>
public sealed class ItemPedidoEntradaValidador : AbstractValidator<ItemPedidoEntrada>
{
    /// <summary>
    /// Define as regras do item.
    /// </summary>
    public ItemPedidoEntradaValidador()
    {
        RuleFor(item => item.ProdutoId)
            .NotEmpty()
            .WithMessage("O produto do item é obrigatório.");

        RuleFor(item => item.NomeProduto)
            .NotEmpty()
            .WithMessage("O nome do produto é obrigatório.");

        RuleFor(item => item.Quantidade)
            .GreaterThan(0)
            .WithMessage("A quantidade do item deve ser maior que zero.");

        RuleFor(item => item.PrecoUnitario)
            .GreaterThanOrEqualTo(0)
            .WithMessage("O preço unitário não pode ser negativo.")
            .Must(preco => decimal.Round(preco, 2, MidpointRounding.AwayFromZero) == preco)
            .WithMessage("O preço unitário aceita no máximo duas casas decimais.");
    }
}
