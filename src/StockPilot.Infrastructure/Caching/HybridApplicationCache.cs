using Microsoft.Extensions.Caching.Hybrid;
using StockPilot.Application.Interfaces.Services;

namespace StockPilot.Infrastructure.Caching;

internal sealed class HybridApplicationCache(HybridCache cache) : IApplicationCache
{
    public ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken)
        => cache.GetOrCreateAsync(
            key,
            factory,
            tags: tags?.ToArray(),
            cancellationToken: cancellationToken);

    public async Task RemoveByTagAsync(string tag, CancellationToken cancellationToken)
        => await cache.RemoveByTagAsync(tag, cancellationToken);
}
