using Confluent.Kafka;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using PlataformaPedidos.Infraestrutura.Mensageria;

namespace PlataformaPedidos.Infraestrutura.Saude;

/// <summary>
/// Verificações de PostgreSQL, Redis e Kafka expostas pela API.
/// </summary>
public static class ExtensoesSaude
{
    /// <summary>
    /// Registra as verificações usadas em <c>/health</c>.
    /// </summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    public static IServiceCollection AdicionarVerificacoesSaude(this IServiceCollection servicos)
    {
        servicos.AddHealthChecks()
            .AddCheck<VerificacaoPostgres>("postgres")
            .AddCheck<VerificacaoRedis>("redis")
            .AddCheck<VerificacaoKafka>("kafka");

        return servicos;
    }

    private sealed class VerificacaoPostgres(IConfiguration configuracao) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var conexaoTexto = configuracao.GetConnectionString("PlataformaPedidos");
            if (string.IsNullOrWhiteSpace(conexaoTexto))
                return HealthCheckResult.Unhealthy("A conexão do PostgreSQL não está configurada.");

            try
            {
                await using var conexao = new NpgsqlConnection(conexaoTexto);
                await conexao.OpenAsync(cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception excecao)
            {
                return HealthCheckResult.Unhealthy("O PostgreSQL não respondeu.", excecao);
            }
        }
    }

    private sealed class VerificacaoRedis(IDistributedCache cache) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await cache.GetStringAsync("saude", cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception excecao)
            {
                return HealthCheckResult.Unhealthy("O Redis não respondeu.", excecao);
            }
        }
    }

    private sealed class VerificacaoKafka(IOptions<OpcoesKafka> opcoes) : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var admin = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = string.IsNullOrWhiteSpace(opcoes.Value.Servidores)
                        ? global::PlataformaPedidos.Infraestrutura.ConexoesLocais.Kafka
                        : opcoes.Value.Servidores
                }).Build();

                var metadados = admin.GetMetadata(TimeSpan.FromSeconds(3));
                return Task.FromResult(metadados.Brokers.Count > 0
                    ? HealthCheckResult.Healthy()
                    : HealthCheckResult.Unhealthy("O Kafka não tem brokers."));
            }
            catch (Exception excecao)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("O Kafka não respondeu.", excecao));
            }
        }
    }
}
