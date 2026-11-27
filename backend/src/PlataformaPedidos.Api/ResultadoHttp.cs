using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Api;

/// <summary>
/// Traduz o resultado de um caso de uso para HTTP e ProblemDetails.
/// </summary>
internal static class ResultadoHttp
{
    public static IResult Para<T>(Resultado<T> resultado, Func<T, IResult> sucesso)
        where T : class
    {
        if (resultado.Sucesso && resultado.Valor is not null)
            return sucesso(resultado.Valor);

        return Problema(resultado.Erro);
    }

    public static IResult Problema(ErroAplicacao? erro)
    {
        if (erro is null)
            return Results.Problem(title: "Erro interno.", statusCode: StatusCodes.Status500InternalServerError);

        var status = erro.Tipo switch
        {
            TipoErroAplicacao.Validacao => StatusCodes.Status400BadRequest,
            TipoErroAplicacao.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErroAplicacao.Conflito => StatusCodes.Status409Conflict,
            TipoErroAplicacao.FalhaExterna => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: Titulo(erro.Tipo),
            detail: erro.Descricao,
            statusCode: status,
            extensions: new Dictionary<string, object?>
            {
                ["codigo"] = erro.Codigo
            });
    }

    private static string Titulo(TipoErroAplicacao tipo) => tipo switch
    {
        TipoErroAplicacao.Validacao => "Entrada inválida.",
        TipoErroAplicacao.NaoEncontrado => "Recurso não encontrado.",
        TipoErroAplicacao.Conflito => "Operação recusada.",
        TipoErroAplicacao.FalhaExterna => "Serviço externo indisponível.",
        _ => "Erro interno."
    };
}
