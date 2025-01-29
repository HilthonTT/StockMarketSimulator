using Microsoft.AspNetCore.Http.Features;
using StockMarketSimulator.Api.Infrastructure;
using System.Diagnostics;

namespace StockMarketSimulator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

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

        services.AddHealthChecks();

        return services;
    }
}
