using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.FeatureManagement;
using Modules.Users.Domain.Enums;
using System.Reflection;
using Web.Api.Endpoints;

namespace Web.Api.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = [.. assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))];

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }

    public static RouteHandlerBuilder HasPermission(this RouteHandlerBuilder app, params Permission[] permissions)
    {
        string[] permissionNames = [.. permissions.Select(p => p.ToString())];

        return app.RequireAuthorization(permissionNames);
    }

    public static RouteHandlerBuilder RequireFeature(this RouteHandlerBuilder app, string featureName)
    {
        return app.AddEndpointFilter(async (context, next) =>
        {
            IFeatureManager featureManager = 
                context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

            if (!await featureManager.IsEnabledAsync(featureName, context))
            {
                return Results.NotFound($"Feature '{featureName}' is disabled");
            }

            return await next(context);
        });
    }
}
