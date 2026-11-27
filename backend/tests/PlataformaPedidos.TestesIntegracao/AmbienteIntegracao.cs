extern alias Api;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PlataformaPedidos.Infraestrutura;
using PlataformaPedidos.Infraestrutura.Mensageria;
using PlataformaPedidos.Infraestrutura.Observabilidade;
using PlataformaPedidos.Infraestrutura.Persistencia;
using PlataformaPedidos.Worker;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace PlataformaPedidos.TestesIntegracao;

/// <summary>
/// Sobe PostgreSQL, Redis, Kafka e WireMock uma vez e hospeda a API e o worker contra eles.
/// </summary>
public sealed class AmbienteIntegracao : IAsyncLifetime
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();
    private readonly RedisContainer _redis = new RedisBuilder("redis:7.2").Build();
    private readonly KafkaContainer _kafka = new KafkaBuilder("confluentinc/cp-kafka:7.5.12").Build();
    private readonly IContainer _wiremock = new ContainerBuilder("wiremock/wiremock:3.13.1")
        .WithPortBinding(8080, true)
        .WithCommand("--port", "8080", "--disable-banner")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(requisicao =>
            requisicao.ForPort(8080).ForPath("/__admin/mappings")))
        .Build();

    private readonly HttpClient _admin = new();
    private WebApplicationFactory<Api::Program>? _fabrica;
    private IHost? _worker;

    public HttpClient Cliente { get; private set; } = null!;

    public string Kafka { get; private set; } = string.Empty;

    public string UrlWireMock { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _kafka.StartAsync(),
            _wiremock.StartAsync());

        Kafka = _kafka.GetBootstrapAddress();
        UrlWireMock = $"http://{_wiremock.Hostname}:{_wiremock.GetMappedPublicPort(8080)}";

        await MigrarAsync();
        await LimparAsync();

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Testing"
        });
        builder.Configuration.AddInMemoryCollection(Configuracao());
        builder.Services.AdicionarInfraestrutura(builder.Configuration);
        builder.Services.AdicionarObservabilidade(builder.Configuration, "PlataformaPedidos.Worker", incluirAspNetCore: false);
        builder.Services.AddHostedService<ProcessadorOutboxWorker>();
        builder.Services.AddHostedService<ConsumidorPagamentoPedidoWorker>();
        _worker = builder.Build();
        await _worker.StartAsync();

        _fabrica = new FabricaApi(Configuracao());
        Cliente = _fabrica.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Cliente.Dispose();
        _fabrica?.Dispose();

        if (_worker is not null)
        {
            await _worker.StopAsync();
            _worker.Dispose();
        }

        _admin.Dispose();
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _kafka.DisposeAsync().AsTask(),
            _wiremock.DisposeAsync().AsTask());
    }

    public async Task LimparAsync()
    {
        await using var contexto = CriarContexto();
        await contexto.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE pedidos, mensagens_outbox, mensagens_processadas CASCADE");
        await _redis.ExecAsync(["redis-cli", "FLUSHDB"]);
        await InstalarStubsPadraoAsync();
    }

    public async Task InstalarCepInstavelAsync()
    {
        using var resposta = await _admin.PostAsync($"{UrlWireMock}/__admin/reset", null);
        resposta.EnsureSuccessStatusCode();

        await PublicarMapeamentoAsync(new
        {
            scenarioName = "cep",
            requiredScenarioState = "Started",
            newScenarioState = "falha-2",
            request = new { method = "GET", urlPath = "/cep/01310100" },
            response = new { status = 503, body = "indisponivel" }
        });
        await PublicarMapeamentoAsync(new
        {
            scenarioName = "cep",
            requiredScenarioState = "falha-2",
            newScenarioState = "ok",
            request = new { method = "GET", urlPath = "/cep/01310100" },
            response = new { status = 503, body = "indisponivel" }
        });
        await PublicarMapeamentoAsync(new
        {
            scenarioName = "cep",
            requiredScenarioState = "ok",
            newScenarioState = "ok",
            request = new { method = "GET", urlPath = "/cep/01310100" },
            response = new { status = 200, jsonBody = EnderecoPadrao() }
        });
    }

    public async Task<int> ContarRequisicoesAsync(string caminho)
    {
        using var resposta = await _admin.GetAsync($"{UrlWireMock}/__admin/requests");
        resposta.EnsureSuccessStatusCode();
        using var documento = await JsonDocument.ParseAsync(await resposta.Content.ReadAsStreamAsync());
        return documento.RootElement.GetProperty("requests").EnumerateArray().Count(item =>
        {
            var url = item.GetProperty("request").GetProperty("url").GetString();
            return url is not null && url.Contains(caminho, StringComparison.Ordinal);
        });
    }

    public PedidosDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        return new PedidosDbContext(opcoes);
    }

    private async Task MigrarAsync()
    {
        await using var contexto = CriarContexto();
        await contexto.Database.MigrateAsync();
    }

    private async Task InstalarStubsPadraoAsync()
    {
        using var resposta = await _admin.PostAsync($"{UrlWireMock}/__admin/reset", null);
        resposta.EnsureSuccessStatusCode();

        await PublicarMapeamentoAsync(new
        {
            request = new { method = "GET", urlPath = "/cep/01310100" },
            response = new { status = 200, jsonBody = EnderecoPadrao() }
        });
        await PublicarMapeamentoAsync(new
        {
            request = new { method = "POST", urlPath = "/pagamentos/autorizar" },
            response = new { status = 200, jsonBody = new { autorizado = true } }
        });
    }

    private async Task PublicarMapeamentoAsync(object mapeamento)
    {
        using var resposta = await _admin.PostAsJsonAsync($"{UrlWireMock}/__admin/mappings", mapeamento, Json);
        resposta.EnsureSuccessStatusCode();
    }

    private Dictionary<string, string?> Configuracao() => new()
    {
        ["ConnectionStrings:PlataformaPedidos"] = _postgres.GetConnectionString(),
        ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
        ["Kafka:Servidores"] = Kafka,
        ["ConsultaCep:UrlBase"] = UrlWireMock,
        ["ConsultaCep:Caminho"] = "cep/{cep}",
        ["GatewayPagamento:UrlBase"] = UrlWireMock,
        ["GatewayPagamento:Caminho"] = "pagamentos/autorizar",
        ["Outbox:Intervalo"] = "00:00:00.200",
        ["OpenTelemetry:EndpointOtlp"] = string.Empty
    };

    private static object EnderecoPadrao() => new
    {
        cep = "01310100",
        logradouro = "Avenida Paulista",
        bairro = "Bela Vista",
        localidade = "São Paulo",
        uf = "SP"
    };

    private sealed class FabricaApi(Dictionary<string, string?> configuracao) : WebApplicationFactory<Api::Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(fontes => fontes.AddInMemoryCollection(configuracao));
        }
    }
}

[CollectionDefinition(nameof(AmbienteIntegracao), DisableParallelization = true)]
public sealed class AmbienteIntegracaoColecao : ICollectionFixture<AmbienteIntegracao>;
