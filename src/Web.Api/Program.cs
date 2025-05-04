using System.Reflection;
using Application;
using Asp.Versioning;
using Asp.Versioning.Builder;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Idempotence;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Modules.Budgeting.Application;
using Modules.Budgeting.BackgroundJobs;
using Modules.Budgeting.Events;
using Modules.Budgeting.Infrastructure;
using Modules.Stocks.Application;
using Modules.Stocks.Infrastructure;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Users.Application;
using Modules.Users.BackgroundJobs;
using Modules.Users.Domain.Repositories;
using Modules.Users.Events;
using Modules.Users.Infrastructure;
using Modules.Users.Infrastructure.Repositories;
using Sentry.OpenTelemetry;
using StockMarketSimulator.ServiceDefaults;
using Web.Api;
using Web.Api.Extensions;

const int apiVersion = 1;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.SendDefaultPii = true;
    options.SampleRate = 1.0f;
    options.UseOpenTelemetry();
});

builder.Services
    .AddSwaggerGenWithAuth()
    .AddPresentation(builder.Configuration);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(builder.Configuration)
    .AddUsersEvents()
    .AddUsersBackgroundJobs();

builder.Services
    .AddStocksApplication()
    .AddStocksInfrastructure(builder.Configuration);

builder.Services
    .AddBudgetingApplication()
    .AddBudgetingInfrastructure(builder.Configuration)
    .AddBudgetingEvents()
    .AddBudgetingBackgroundJobs();

builder.Services.Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));

builder.Services.AddBackgroundJobs();
builder.Services.AddVersioning();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

WebApplication app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(apiVersion))
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

app.UseRequestContextLogging();

app.UseStatusCodePages();

app.UseRateLimiter();

app.UseResponseCompression();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
public partial class Program;
