using StockMarketSimulator.AppHost;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("stockmarketsimulator-db")
    .WithDataVolume()
    .WithPgAdmin();

IResourceBuilder<RedisResource> redis = builder.AddRedis("stockmarketsimulator-redis")
    .WithDataVolume();

IResourceBuilder<RabbitMQServerResource> rabbitMq = builder.AddRabbitMQ("stockmarketsimulator-rabbitmq")
    .WithDataVolume();

builder.AddProject<Projects.Web_Api>("web-api")
    .WithSwaggerUI()
    .WithScalar()
    .WithRedoc()
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitMq)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitMq);

await builder.Build().RunAsync();
