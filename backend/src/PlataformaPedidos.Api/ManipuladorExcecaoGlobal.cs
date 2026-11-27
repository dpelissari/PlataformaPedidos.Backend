using Microsoft.AspNetCore.Diagnostics;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Api;

/// <summary>
/// Converte exceção de domínio e falha inesperada em ProblemDetails, sem detalhe interno no erro genérico.
/// </summary>
internal sealed class ManipuladorExcecaoGlobal(ILogger<ManipuladorExcecaoGlobal> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ExcecaoDominio dominio)
        {
            var problema = ResultadoHttp.Problema(
                ErroAplicacao.Conflito("pedido.regra_violada", dominio.Message));
            await problema.ExecuteAsync(httpContext);
            return true;
        }

        logger.LogError(exception, "Falha não tratada na API.");
        var interno = Results.Problem(
            title: "Erro interno.",
            detail: "A operação não pôde ser concluída.",
            statusCode: StatusCodes.Status500InternalServerError);
        await interno.ExecuteAsync(httpContext);
        return true;
    }
}
