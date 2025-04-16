using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Infrastructure.Database;

namespace Api.FunctionalTests.Abstractions;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly FunctionalTestWebAppFactory Factory;
    protected readonly HttpClient Client;

    protected BaseFunctionalTest(FunctionalTestWebAppFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<User?> GetUserByIdAsync(Guid id)
    {
        using var scope = Factory.Services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await usersDbContext.Users.FindAsync(id);
    }

    protected async Task<Guid> CreateUserAsync()
    {
        var request = new RegisterRequest("test@test.com", "username", "password");

        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        await usersDbContext.Users.ExecuteDeleteAsync();
    }
}
