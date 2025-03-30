using StockMarketSimulator.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("stockmarketsimulator-db")
    .WithDataVolume()
    .WithPgAdmin();

IResourceBuilder<RedisResource> redis = builder.AddRedis("stockmarketsimulator-redis")
    .WithDataVolume();

builder.AddProject<Projects.Web_Api>("web-api")
    .WithSwaggerUI()
    .WithScalar()
    .WithRedoc()
    .WithReference(postgres)
    .WithReference(redis)
    .WaitFor(postgres)
    .WaitFor(redis);

await builder.Build().RunAsync();
