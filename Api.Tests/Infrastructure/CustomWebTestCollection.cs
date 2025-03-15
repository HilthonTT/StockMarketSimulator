namespace Api.Tests.Infrastructure;

[CollectionDefinition(nameof(CustomWebTestCollection))]
public sealed class CustomWebTestCollection : ICollectionFixture<CustomWebAppFactory>
{
}
