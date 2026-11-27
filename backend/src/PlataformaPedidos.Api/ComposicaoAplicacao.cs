using FluentValidation;
using PlataformaPedidos.Aplicacao.Enderecos;
using PlataformaPedidos.Aplicacao.Pedidos;

namespace PlataformaPedidos.Api;

/// <summary>
/// Registra os casos de uso e os validadores chamados pelos endpoints.
/// </summary>
internal static class ComposicaoAplicacao
{
    public static IServiceCollection AdicionarCasosDeUso(this IServiceCollection servicos)
    {
        servicos.AddScoped<CriarPedidoManipulador>();
        servicos.AddScoped<SolicitarPagamentoPedidoManipulador>();
        servicos.AddScoped<CancelarPedidoManipulador>();
        servicos.AddScoped<ObterPedidoPorIdManipulador>();
        servicos.AddScoped<ListarPedidosManipulador>();
        servicos.AddScoped<ConsultarEnderecoPorCepManipulador>();

        servicos.AddScoped<IValidator<CriarPedidoComando>, CriarPedidoComandoValidador>();
        servicos.AddScoped<IValidator<SolicitarPagamentoPedidoComando>, SolicitarPagamentoPedidoComandoValidador>();
        servicos.AddScoped<IValidator<CancelarPedidoComando>, CancelarPedidoComandoValidador>();
        servicos.AddScoped<IValidator<ObterPedidoPorIdConsulta>, ObterPedidoPorIdConsultaValidador>();
        servicos.AddScoped<IValidator<ListarPedidosConsulta>, ListarPedidosConsultaValidador>();
        servicos.AddScoped<IValidator<ConsultarEnderecoPorCepConsulta>, ConsultarEnderecoPorCepConsultaValidador>();

        return servicos;
    }
}
