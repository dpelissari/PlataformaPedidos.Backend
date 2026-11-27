using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace PlataformaPedidos.Infraestrutura.Observabilidade;

/// <summary>
/// Rastreamento e métricas exportados para o Aspire Dashboard.
/// </summary>
public static class ExtensoesObservabilidade
{
    public const string Secao = "OpenTelemetry";

    /// <summary>
    /// Registra traces de HTTP, EF Core e da aplicação, além das métricas de runtime.
    /// </summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <param name="configuracao">Configuração da aplicação.</param>
    /// <param name="nomeServico">Nome do serviço no recurso de telemetria.</param>
    /// <param name="incluirAspNetCore">Inclui a instrumentação do pipeline HTTP quando o host é a API.</param>
    public static IServiceCollection AdicionarObservabilidade(
        this IServiceCollection servicos,
        IConfiguration configuracao,
        string nomeServico,
        bool incluirAspNetCore)
    {
        var endpoint = ResolverEndpoint(configuracao["OpenTelemetry:EndpointOtlp"]);

        servicos.AddOpenTelemetry()
            .ConfigureResource(recurso => recurso.AddService(nomeServico))
            .WithTracing(traces =>
            {
                traces
                    .AddSource(Telemetria.Nome)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();

                if (incluirAspNetCore)
                    traces.AddAspNetCoreInstrumentation();

                if (endpoint is not null)
                    traces.AddOtlpExporter(opcoes => ConfigurarExportador(opcoes, endpoint));
            })
            .WithMetrics(metricas =>
            {
                metricas
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("Microsoft.EntityFrameworkCore");

                if (incluirAspNetCore)
                    metricas.AddAspNetCoreInstrumentation();

                if (endpoint is not null)
                    metricas.AddOtlpExporter(opcoes => ConfigurarExportador(opcoes, endpoint));
            });

        return servicos;
    }

    private static Uri? ResolverEndpoint(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return Uri.TryCreate(valor.Trim(), UriKind.Absolute, out var uri) ? uri : null;
    }

    private static void ConfigurarExportador(OtlpExporterOptions opcoes, Uri endpoint)
    {
        opcoes.Endpoint = endpoint;
        opcoes.Protocol = OtlpExportProtocol.Grpc;
    }
}
