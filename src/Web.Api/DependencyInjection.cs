using Microsoft.AspNetCore.RateLimiting;
using Infrastructure;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.FeatureManagement;
using SharedKernel;
using StackExchange.Redis;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.RateLimiting;
using Web.Api.Endpoints;
using Web.Api.Features;
using Web.Api.Infrastructure;
using Web.Api.Middleware;
using Web.Api.RateLimiting;

namespace Web.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerServices();
        services.AddCors();
        services.AddExceptionHandling();
        services.AddMiddlewares();
        services.ConfigureRateLimiter();

        services.AddFeatureManagement()
            .WithTargeting<UserTargetingContext>();

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    private static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        return services;
    }

    private static IServiceCollection AddMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<UserContextEnrichementMiddleware>();
        services.AddTransient<RequestContextLoggingMiddleware>();

        return services;
    }

    private static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(_ => { });

        services.AddOptions<RateLimiterOptions>().Configure<IServiceProvider>((options, serviceProvider) =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // The process-wide multiplexer from the Infrastructure layer. It is missing when Redis was
            // unreachable at start-up (Postgres-cache fallback mode); the limiter then runs in-process.
            IConnectionMultiplexer? connectionMultiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
            ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Web.Api.RateLimiting");

            options.AddPolicy(RateLimiterPolicyNames.GlobalLimiter, _ =>
                RateLimitPartition.Get(RateLimiterPolicyNames.GlobalLimiter, key =>
                    GlobalLimiterFactory.Create(key, connectionMultiplexer, logger)));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).",
                        cancellationToken);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                       "Too many requests. Please try again later.",
                       cancellationToken);
                }
            };
        });

        return services;
    }
}
