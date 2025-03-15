using Api.Tests.Infrastructure;
using Dapper;
using StockMarketSimulator.Api.Modules.Users.Contracts;
using StockMarketSimulator.Api.Modules.Users.Domain;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests;

public sealed class UserEndpointsTests : BaseIntegrationTest
{
    public UserEndpointsTests(CustomWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegisterUser_WithValidInput_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterUserRequest("test@test.com", "username", "AbC-123", "AbC-123");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        User? savedUser = await GetUserByEmailAsync(request.Email);
        Assert.NotNull(savedUser);
    }

    [Fact]
    public async Task RegisterUser_WithBadInput_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterUserRequest("not-email", "username", "weakpassword", "weakpassword");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        User? savedUser = await GetUserByEmailAsync(request.Email);
        Assert.Null(savedUser);
    }

    private async Task<User?> GetUserByEmailAsync(string email)
    {
        await using var connection = await DbConnectionFactory.GetOpenConnectionAsync();

        const string sql =
            """
            SELECT 
                id AS Id,
                email AS Email,
                user_name AS Username,
                password_hash AS PasswordHash
            FROM public.users
            WHERE email = @Email;
            """;

        return await connection.QueryFirstOrDefaultAsync<User>(
           sql,
           new { Email = email });
    }
}
