using Asp.Versioning.Builder;
using Asp.Versioning;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using StockMarketSimulator.Api;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Modules.Users;
using StockMarketSimulator.Api.Modules.Stocks;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;
using StockMarketSimulator.Api.Modules.Transactions;
using StockMarketSimulator.Api.Modules.Budgets;
using StockMarketSimulator.Api.Modules.Roles;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddUserModule(builder.Configuration)
    .AddStocksModule(builder.Configuration)
    .AddPresentation(builder.Configuration)
    .AddTransactionsModule()
    .AddBudgetModule()
    .AddRolesModule();

builder.Services.AddEndpoints(PresentationAssembly.Instance);

WebApplication app = builder.Build();

app.UseResponseCompression();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerWithUi();

    app.UseCors(policy => policy
        .WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHub<StocksFeedHub>("/stocks-feed");

app.UseRateLimiter();

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

// Add the user context enrichment middleware after authentication
app.UseUserContextEnrichment();

app.UseExceptionHandler();

app.UseStatusCodePages();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
public partial class Program;
