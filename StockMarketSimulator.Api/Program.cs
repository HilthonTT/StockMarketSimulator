using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using StockMarketSimulator.Api;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure.Events;
using StockMarketSimulator.Api.Modules.Users;
using StockMarketSimulator.Api.Modules.Users.Application.Register;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

builder.Services.AddSwaggerGenWithAuth();

builder.Services
    .AddUserModule(builder.Configuration)
    .AddPresentation(builder.Configuration);

WebApplication app = builder.Build();

app.MapPost("/users", async (RegisterUserRequest request, IEventBus eventBus, CancellationToken cancellationToken) =>
{
    var result = await eventBus.SendAsync<RegisterUserCommand, RegisterUserResponse>(
        new RegisterUserCommand(request.Email, request.Username, request.Password),
        cancellationToken
    );

    if (result.IsSuccess)
    {
        return Results.Ok(new { UserId = result.Value.UserId });
    }

    return Results.BadRequest();
})
.WithTags("Users");

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

app.UseAuthentication();

app.UseAuthorization();

app.UseExceptionHandler();

app.UseStatusCodePages();

await app.RunAsync();

// REMARK: Required for functional and integration tests to work.
public partial class Program;
