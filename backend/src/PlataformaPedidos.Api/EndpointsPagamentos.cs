using Microsoft.Extensions.Options;
using PlataformaPedidos.Infraestrutura.Integracoes;

namespace PlataformaPedidos.Api;

/// <summary>
/// Dados públicos do Mercado Pago para o navegador gerar o token do cartão.
/// </summary>
internal static class EndpointsPagamentos
{
    public static IEndpointRouteBuilder MapearPagamentos(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/pagamentos/configuracao", Consultar)
            .WithTags("Pagamentos")
            .WithName("ConsultarConfiguracaoPagamento")
            .Produces<ConfiguracaoPagamentoResposta>();

        return app;
    }

    private static IResult Consultar(IOptions<OpcoesMercadoPago> opcoes) =>
        Results.Ok(new ConfiguracaoPagamentoResposta(opcoes.Value.ChavePublica));
}

/// <summary>
/// Public Key do Mercado Pago. O Access Token não sai da API.
/// </summary>
/// <param name="ChavePublica">Chave pública de teste ou de produção.</param>
internal sealed record ConfiguracaoPagamentoResposta(string ChavePublica);
