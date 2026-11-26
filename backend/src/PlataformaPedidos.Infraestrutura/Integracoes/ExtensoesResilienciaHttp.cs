using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Timeout, retry e circuit breaker compartilhados pelos clientes de CEP e pagamento.
/// </summary>
internal static class ExtensoesResilienciaHttp
{
    private static readonly TimeSpan TimeoutDaTentativa = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Aplica timeout de 3 segundos, retry exponencial com jitter só em falha transitória e circuit breaker.
    /// </summary>
    public static void AdicionarResilienciaHttp(this IHttpClientBuilder cliente) =>
        cliente.AddStandardResilienceHandler(opcoes =>
        {
            opcoes.AttemptTimeout.Timeout = TimeoutDaTentativa;
            opcoes.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            opcoes.Retry.MaxRetryAttempts = 3;
            opcoes.Retry.BackoffType = DelayBackoffType.Exponential;
            opcoes.Retry.UseJitter = true;
            opcoes.Retry.Delay = TimeSpan.FromMilliseconds(200);
            opcoes.Retry.ShouldHandle = FalhaTransitoria;
            opcoes.CircuitBreaker.ShouldHandle = FalhaTransitoria;
            opcoes.CircuitBreaker.FailureRatio = 0.5;
            opcoes.CircuitBreaker.MinimumThroughput = 10;
            opcoes.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            opcoes.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        });

    private static ValueTask<bool> FalhaTransitoria(
        Polly.CircuitBreaker.CircuitBreakerPredicateArguments<HttpResponseMessage> argumentos) =>
        new(EhTransitoria(argumentos.Outcome));

    private static ValueTask<bool> FalhaTransitoria(
        Polly.Retry.RetryPredicateArguments<HttpResponseMessage> argumentos) =>
        new(EhTransitoria(argumentos.Outcome));

    private static bool EhTransitoria(Polly.Outcome<HttpResponseMessage> resultado)
    {
        if (resultado.Exception is HttpRequestException or Polly.Timeout.TimeoutRejectedException)
            return true;

        if (resultado.Result is not HttpResponseMessage resposta)
            return false;

        var codigo = (int)resposta.StatusCode;
        return codigo is 408 or 429 || codigo >= 500;
    }
}
