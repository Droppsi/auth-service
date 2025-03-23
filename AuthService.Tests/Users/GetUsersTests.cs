using System.Net;
using System.Net.Http.Json;
using AuthService.Contract.Responses.Users;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class GetUsersTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_200_OK_With_Users()
    {
        ClearDb();

        // Arrange
        var user = new User { Id = Guid.NewGuid(), Username = "Username", Password = "<PASSWORD>" };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");
        var content = await response.Content.ReadFromJsonAsync<UsersResponse>();
        content.ShouldNotBeNull();
        content.Users.ShouldNotBeNull().ShouldNotBeEmpty();
        content.Users.Count().ShouldBe(1);
        content.Users.First().Id.ShouldBe(user.Id);
        content.Users.First().Username.ShouldBe(user.Username);

        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldNotBeEmpty();
        DbContext.Users.Count().ShouldBe(1);
        DbContext.Users.First().Id.ShouldBe(user.Id);
        DbContext.Users.First().Username.ShouldBe(user.Username);
    }

    [Fact]
    public async Task Should_Return_200_OK_With_Empty_List_When_No_Users_Exist()
    {
        ClearDb();
        // Arrange

        // Act
        HttpResponseMessage response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.ShouldNotBeNull();
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");
        var content = await response.Content.ReadFromJsonAsync<UsersResponse>();
        content.ShouldNotBeNull();
        content.Users.ShouldNotBeNull().ShouldBeEmpty();
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }
}