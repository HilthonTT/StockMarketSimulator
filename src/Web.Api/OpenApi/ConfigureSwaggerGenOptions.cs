﻿using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Api.OpenApi;

internal sealed class ConfigureSwaggerGenOptions(IApiVersionDescriptionProvider provider) 
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription apiVersionDescription)
    {
        var openApiInfo = new OpenApiInfo
        {
            Title = $"StockMarketSimulator.Api v{apiVersionDescription.ApiVersion}",
            Version = apiVersionDescription.ApiVersion.ToString(),
        };

        if (apiVersionDescription.IsDeprecated)
        {
            openApiInfo.Description += " This API version has been deprecated.";
        }

        return openApiInfo;
    }
}
