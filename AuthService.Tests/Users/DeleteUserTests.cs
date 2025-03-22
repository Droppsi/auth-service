using System.Net;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class DeleteUserTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_200_OK_When_Deleting_User()
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
        HttpResponseMessage response = await Client.DeleteAsync($"/api/users/{user.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_404_NotFound_When_Deleting_User_That_Does_Not_Exist()
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
        HttpResponseMessage response = await Client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldNotBeEmpty();
    }
}