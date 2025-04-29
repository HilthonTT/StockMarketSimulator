using System.Net;
using System.Net.Http.Json;
using Api.FunctionalTests.Abstractions;
using Api.FunctionalTests.Contracts;
using Api.FunctionalTests.Extensions;
using Modules.Users.Application.Core.Errors;
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

        CustomProblemDetails problemDetails = await response.GetProblemDetails();

        Assert.Contains(UsersValidationErrors.RegisterUser.EmailIsRequired, problemDetails.Errors);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenEmailFormatIsInvalid()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("not-an-email", "username", "password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        CustomProblemDetails problemDetails = await response.GetProblemDetails();

        Assert.Contains(UsersValidationErrors.RegisterUser.EmailFormatIsInvalid, problemDetails.Errors);
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

        CustomProblemDetails problemDetails = await response.GetProblemDetails();

        Assert.Contains(UsersValidationErrors.RegisterUser.UsernameIsRequired, problemDetails.Errors);
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

        CustomProblemDetails problemDetails = await response.GetProblemDetails();

        Assert.Contains(UsersValidationErrors.RegisterUser.PasswordIsRequired, problemDetails.Errors);
        Assert.Contains(UsersValidationErrors.RegisterUser.PasswordIsTooShort, problemDetails.Errors);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_WhenPasswordIsTooShort()
    {
        await CleanDatabaseAsync();

        // Arrange
        var request = new RegisterRequest("email@email.com", "username", "short");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        CustomProblemDetails problemDetails = await response.GetProblemDetails();

        Assert.Contains(UsersValidationErrors.RegisterUser.PasswordIsTooShort, problemDetails.Errors);
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
