namespace Application.Abstractions.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically claims <paramref name="key"/> for <paramref name="expiration"/>.
    /// </summary>
    /// <returns><c>true</c> if this caller took the claim; <c>false</c> if an unexpired claim already exists.</returns>
    /// <remarks>
    /// Claims live in their own keyspace: use a key that is never read with <see cref="GetAsync{T}"/>, and
    /// release a claim early with <see cref="ReleaseClaimAsync"/>.
    /// </remarks>
    Task<bool> TryClaimAsync(string key, TimeSpan expiration, CancellationToken cancellationToken = default);

    /// <summary>Releases a claim taken with <see cref="TryClaimAsync"/>.</summary>
    Task ReleaseClaimAsync(string key, CancellationToken cancellationToken = default);

    Task<T> GetOrCreateAsync<T>(
        string key, 
        Func<Task<T>> factory, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default);
}
