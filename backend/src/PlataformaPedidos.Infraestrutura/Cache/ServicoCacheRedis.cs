using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using PlataformaPedidos.Aplicacao.Contratos;

namespace PlataformaPedidos.Infraestrutura.Cache;

/// <summary>
/// Cache distribuído no Redis, com JSON e expiração informada por quem grava.
/// </summary>
public sealed class ServicoCacheRedis(IDistributedCache cache) : IServicoCache
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<T?> ObterAsync<T>(string chave, CancellationToken cancellationToken = default)
        where T : class
    {
        var conteudo = await cache.GetStringAsync(chave, cancellationToken);
        return conteudo is null ? null : JsonSerializer.Deserialize<T>(conteudo, Json);
    }

    /// <inheritdoc />
    public Task DefinirAsync<T>(string chave, T valor, TimeSpan expiracao, CancellationToken cancellationToken = default)
        where T : class
    {
        var conteudo = JsonSerializer.Serialize(valor, Json);
        var opcoes = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiracao
        };

        return cache.SetStringAsync(chave, conteudo, opcoes, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoverAsync(string chave, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(chave, cancellationToken);
}
