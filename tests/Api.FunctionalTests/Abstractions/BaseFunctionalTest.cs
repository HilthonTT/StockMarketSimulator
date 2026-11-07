using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Infrastructure.Database;

namespace Api.FunctionalTests.Abstractions;

public abstract class BaseFunctionalTest : IClassFixture<FunctionalTestWebAppFactory>
{
    protected const string Password = "Passw0rd!123";

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
        var request = new RegisterRequest("test@test.com", "username", Password);

        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/authentication/register", request);

        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>
    /// Registers a user, marks the email as verified directly in the database (no verification email or
    /// domain events), logs in and returns the user id with the issued tokens.
    /// </summary>
    protected async Task<(Guid UserId, TokenResponse Tokens)> RegisterAndLoginAsync()
    {
        string unique = Guid.NewGuid().ToString("N")[..12];
        string email = $"user-{unique}@test.com";

        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync(
            "api/v1/authentication/register",
            new RegisterRequest(email, $"user{unique}", Password));
        registerResponse.EnsureSuccessStatusCode();

        Guid userId = await registerResponse.Content.ReadFromJsonAsync<Guid>();

        using (IServiceScope scope = Factory.Services.CreateScope())
        {
            UsersDbContext usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            // What VerifyEmailCommandHandler does, minus the domain event (welcome email, budget creation).
            await usersDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE users.users SET email_verified = true WHERE id = {userId}");

            await usersDbContext.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO users.role_user (roles_id, user_id) VALUES ({Role.Registered.Id}, {userId}) ON CONFLICT DO NOTHING");
        }

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            "api/v1/authentication/login",
            new LoginRequest(email, Password));
        loginResponse.EnsureSuccessStatusCode();

        TokenResponse tokens = (await loginResponse.Content.ReadFromJsonAsync<TokenResponse>())!;

        return (userId, tokens);
    }

    protected HttpClient CreateAuthenticatedClient(string accessToken)
    {
        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }

    protected async Task CleanDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var usersDbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        await usersDbContext.Users.ExecuteDeleteAsync();
    }
}
