using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// A listagem não recebe filtro. O validador mantém o mesmo contrato dos demais casos de uso.
/// </summary>
public sealed class ListarPedidosConsultaValidador : AbstractValidator<ListarPedidosConsulta>
{
}
