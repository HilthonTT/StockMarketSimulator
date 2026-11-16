using StockMarketSimulator.ServiceDefaults;
using Yarp.Proxy;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.AllowAnyClientCertificate();
    });
});

builder.AddServiceDefaults();

builder.Services.AddOpenApi();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.ConfigureRateLimiter();

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    // Credentialed CORS must name its origins: allowing every origin with credentials would let any website
    // make authenticated calls on a visitor's behalf.
    string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() is { Length: > 0 } origins
        ? origins
        : [builder.Configuration["Cors:AllowedOrigin"] ?? "http://localhost:3000"];

    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseCors();

app.UseWebSockets();

app.MapReverseProxy();

app.MapHealthChecks("health");

await app.RunAsync();
