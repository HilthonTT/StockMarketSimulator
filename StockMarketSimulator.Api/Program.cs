using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using StockMarketSimulator.Api;
using StockMarketSimulator.Api.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services.AddPresentation();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerWithUi();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHttpsRedirection();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseStatusCodePages();

await app.RunAsync();
