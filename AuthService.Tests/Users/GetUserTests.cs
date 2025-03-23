using System.Net;
using System.Net.Http.Json;
using AuthService.Contract.Responses.Users;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class GetUserTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_200_OK_When_User_Exists()
    {
        ClearDb();

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "Username",
            Password = "<PASSWORD>"
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/users/{user.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");
        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        content.ShouldNotBeNull();
        content.Username.ShouldBe(user.Username);
        content.Id.ShouldBe(user.Id);

        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldNotBeEmpty();
        DbContext.Users.Count().ShouldBe(1);
        DbContext.Users.First().Id.ShouldBe(user.Id);
        DbContext.Users.First().Username.ShouldBe(user.Username);
    }

    [Fact]
    public async Task Should_Return_404_NotFound_When_User_Does_Not_Exist()
    {
        ClearDb();

        // Arrange

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }
}