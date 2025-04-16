using System.Net;
using System.Net.Http.Json;
using AuthService.Contract.Requests.Users;
using AuthService.Contract.Responses.Users;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class LoginUserTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_Auth_And_Refresh_Token_When_User_Exists_With_Correct_Credentials()
    {
        ClearDb();

        // Arrange
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Username = "Username",
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password")
        };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new LoginUserRequest("Username", "Password");

        // Act
        HttpResponseMessage responseMessage = await Client.PostAsJsonAsync("/api/users/login", request);

        // Assert
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        var response = await responseMessage.Content.ReadFromJsonAsync<LoginResponse>();
        response.ShouldNotBeNull();
        response.AccessToken.ShouldNotBeNullOrEmpty();
        response.RefreshToken.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Should_Return_401_Unauthorized_When_User_Exists_With_Incorrect_Credentials()
    {
        ClearDb();
        // Arrange
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Username = "Username",
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password")
        };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        var request = new LoginUserRequest("Username", "WrongPassword");

        // Act
        HttpResponseMessage responseMessage = await Client.PostAsJsonAsync("/api/users/login", request);

        // Assert
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Should_Return_404_NotFound_When_User_Does_Not_Exist()
    {
        ClearDb();
        // Arrange
        var request = new LoginUserRequest("Username", "Password");

        // Act
        HttpResponseMessage responseMessage = await Client.PostAsJsonAsync("/api/users/login", request);

        // Assert
        responseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}