using PlataformaPedidos.Aplicacao.Enderecos;

namespace PlataformaPedidos.Api;

/// <summary>
/// Consulta de endereço pelo CEP.
/// </summary>
internal static class EndpointsEnderecos
{
    public static IEndpointRouteBuilder MapearEnderecos(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/enderecos/{cep}", Consultar)
            .WithTags("Endereços")
            .WithName("ConsultarEnderecoPorCep")
            .Produces<EnderecoPorCepResposta>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return app;
    }

    private static async Task<IResult> Consultar(
        string cep,
        ConsultarEnderecoPorCepManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(new ConsultarEnderecoPorCepConsulta(cep), cancellationToken);
        return ResultadoHttp.Para(resultado, Results.Ok);
    }
}
