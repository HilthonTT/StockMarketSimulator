using Application.Abstractions.Messaging;
using SharedKernel;
using Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Behaviors;

public static class QueryCachingDecorator
{
    public sealed class QueryHandler<TQuery, TResponse>(
       IQueryHandler<TQuery, TResponse> innerHandler,
       ICacheService cacheService,
       ILogger<QueryHandler<TQuery, TResponse>> logger)
       : IQueryHandler<TQuery, TResponse>
       where TQuery : ICachedQuery<TResponse>
    {
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            TResponse? cachedResult = await cacheService.GetAsync<TResponse>(
                query.CacheKey,
                cancellationToken);

            string requestName = typeof(TQuery).Name;
            if (cachedResult is not null)
            {
                logger.LogInformation("Cache hit for {RequestName}", requestName);

                return cachedResult;
            }

            logger.LogInformation("Cache miss for {RequestName}", requestName);

            Result<TResponse> result = await innerHandler.Handle(query, cancellationToken);

            if (result.IsSuccess)
            {
                await cacheService.SetAsync(
                    query.CacheKey,
                    result,
                    query.Expiration,
                    cancellationToken);
            }

            return result;
        }
    }
}
