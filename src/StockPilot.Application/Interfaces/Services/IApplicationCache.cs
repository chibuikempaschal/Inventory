namespace StockPilot.Application.Interfaces.Services;

/// <summary>Application-facing cache abstraction. Infrastructure supplies the cache provider.</summary>
public interface IApplicationCache
{
    ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> factory,
        IEnumerable<string>? tags,
        CancellationToken cancellationToken);

    Task RemoveByTagAsync(string tag, CancellationToken cancellationToken);
}
