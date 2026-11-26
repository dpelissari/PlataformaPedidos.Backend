using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Valida a consulta de pedido por identificador.
/// </summary>
public sealed class ObterPedidoPorIdConsultaValidador : AbstractValidator<ObterPedidoPorIdConsulta>
{
    /// <summary>
    /// Define as regras da consulta.
    /// </summary>
    public ObterPedidoPorIdConsultaValidador()
    {
        RuleFor(consulta => consulta.PedidoId)
            .NotEmpty()
            .WithMessage("O pedido é obrigatório.");
    }
}
