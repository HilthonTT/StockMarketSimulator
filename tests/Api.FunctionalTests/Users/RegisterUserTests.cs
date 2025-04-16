using System.Net;
using System.Net.Http.Json;
using Api.FunctionalTests.Abstractions;
using Modules.Users.Contracts.Users;

namespace Api.FunctionalTests.Users;

public sealed class RegisterUserTests : BaseFunctionalTest
{
    public RegisterUserTests(FunctionalTestWebAppFactory factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenEmailIsMissing()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("", "username", "password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenUsernameIsMissing()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("email@email.com", "", "password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenPasswordIsMissing()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("email@email.com", "username", "");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnOk_WhenRequestIsValid()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("email@email.com", "username", "password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_WhenUserExists()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("email@email.com", "username", "password");

        // Act
        await Client.PostAsJsonAsync("api/v1/users/register", request);

        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
