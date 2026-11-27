using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Dominio.Pedidos;
using PlataformaPedidos.Infraestrutura.Cache;
using PlataformaPedidos.Infraestrutura.Integracoes;
using PlataformaPedidos.Infraestrutura.Mensageria;
using PlataformaPedidos.Infraestrutura.Persistencia;

namespace PlataformaPedidos.Infraestrutura;

/// <summary>
/// Registra persistência, cache, clientes HTTP resilientes e o produtor Kafka.
/// </summary>
public static class ExtensoesInfraestrutura
{
    /// <summary>
    /// Adiciona os adaptadores de infraestrutura usados pela API e pelo worker.
    /// </summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <param name="configuracao">Configuração da aplicação.</param>
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        servicos.AddSingleton<InterceptadorOutbox>();
        servicos.AddDbContext<PedidosDbContext>((provedor, opcoes) =>
        {
            var postgres = configuracao.GetConnectionString("PlataformaPedidos");
            if (string.IsNullOrWhiteSpace(postgres))
                postgres = ConexoesLocais.Postgres;

            opcoes.UseNpgsql(postgres);
            opcoes.AddInterceptors(provedor.GetRequiredService<InterceptadorOutbox>());
        });

        servicos.AddScoped<IPedidoRepositorio, PedidoRepositorio>();
        servicos.AddScoped<IUnidadeTrabalho, UnidadeTrabalho>();

        servicos.AddStackExchangeRedisCache(opcoes =>
        {
            var redis = configuracao.GetConnectionString("Redis");
            if (string.IsNullOrWhiteSpace(redis))
                redis = ConexoesLocais.Redis;

            opcoes.Configuration = redis;
        });
        servicos.AddSingleton<IServicoCache, ServicoCacheRedis>();

        servicos.Configure<OpcoesConsultaCep>(configuracao.GetSection(OpcoesConsultaCep.Secao));
        servicos.Configure<OpcoesGatewayPagamento>(configuracao.GetSection(OpcoesGatewayPagamento.Secao));
        servicos.Configure<OpcoesKafka>(configuracao.GetSection(OpcoesKafka.Secao));

        servicos.AddHttpClient<IServicoConsultaCep, ServicoConsultaCepHttp>((provedor, cliente) =>
        {
            var opcoes = provedor.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcoesConsultaCep>>().Value;
            cliente.BaseAddress = CriarUrlBase(opcoes.UrlBase);
        }).AdicionarResilienciaHttp();

        servicos.AddHttpClient<IGatewayPagamento, GatewayPagamentoHttp>((provedor, cliente) =>
        {
            var opcoes = provedor.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcoesGatewayPagamento>>().Value;
            cliente.BaseAddress = CriarUrlBase(opcoes.UrlBase);
        }).AdicionarResilienciaHttp();

        servicos.AddSingleton<ProdutorEventosKafka>();
        servicos.AddSingleton<IBarramentoEventos>(provedor => provedor.GetRequiredService<ProdutorEventosKafka>());
        servicos.Configure<OpcoesOutbox>(configuracao.GetSection(OpcoesOutbox.Secao));
        servicos.AddSingleton<PublicadorOutbox>();
        servicos.AddSingleton<ConsumidorPagamentoPedido>();

        return servicos;
    }

    private static Uri CriarUrlBase(string? urlBase)
    {
        var url = string.IsNullOrWhiteSpace(urlBase) ? ConexoesLocais.WireMock : urlBase.Trim();
        if (!url.EndsWith('/'))
            url += "/";

        return new Uri(url);
    }
}
