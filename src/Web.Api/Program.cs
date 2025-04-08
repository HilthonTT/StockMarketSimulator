using System.Reflection;
using Application;
using Asp.Versioning;
using Asp.Versioning.Builder;
using HealthChecks.UI.Client;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Modules.Budgeting.Application;
using Modules.Budgeting.Infrastructure;
using Modules.Stocks.Application;
using Modules.Stocks.Infrastructure;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Users.Application;
using Modules.Users.Infrastructure;
using Web.Api;
using Web.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddSwaggerGenWithAuth()
    .AddPresentation();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration);

builder.Services
    .AddStocksApplication()
    .AddStocksInfrastructure(builder.Configuration);

builder.Services
    .AddBudgetingApplication()
    .AddBudgetingInfrastructure(builder.Configuration);

builder.Services.AddBackgroundJobs();
builder.Services.AddVersioning();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapDefaultEndpoints();

app.MapEndpoints(versionedGroup);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerWithUi();

    app.ApplyMigrations();

    app.UseCorsConfiguration(builder.Configuration);
}

app.UseHttpsRedirection();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHub<StocksFeedHub>("/stocks-feed");

app.UseExceptionHandler();

app.UseAuthentication();

app.UseAuthorization();

// Add the user context enrichment middleware after authentication
app.UseUserContextEnrichment();

app.UseStatusCodePages();

await app.RunAsync();

public partial class Program;
