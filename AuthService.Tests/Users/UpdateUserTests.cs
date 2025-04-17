using System.Net;
using System.Net.Http.Json;
using AuthService.Contract.Requests.Users;
using AuthService.Contract.Responses.Users;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class UpdateUserTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_200_OK_When_Changing_Username_Of_Existing_User()
    {
        ClearDb();

        // Arrange
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Username = "Username",
            Password = "<PASSWORD>"
        };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        UpdateUserRequest request = new("New Username");
        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<UserResponse>();
        content.ShouldNotBeNull();
        content.Username.ShouldBe("New Username");
        content.Id.ShouldBe(user.Id);

        RefreshDbContext();
        User? updatedUser = DbContext.Users.SingleOrDefault(u => u.Id == user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.Username.ShouldBe("New Username");
        updatedUser.Password.ShouldBe(user.Password);
        updatedUser.Id.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Changing_Username_Of_Existing_User_To_Empty_String()
    {
        ClearDb();

        // Arrange
        var user = new User()
        {
            Id = Guid.NewGuid(),
            Username = "Username",
            Password = "<PASSWORD>"
        };
        await DbContext.Users.AddAsync(user);
        await DbContext.SaveChangesAsync();

        UpdateUserRequest request = new("");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        User? updatedUser = DbContext.Users.SingleOrDefault(u => u.Id == user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.Username.ShouldBe("Username");
        updatedUser.Password.ShouldBe(user.Password);
        updatedUser.Id.ShouldBe(user.Id);
    }

    [Fact]
    public async Task Should_Return_404_NotFound_When_Changing_Username_Of_Non_Existing_User()
    {
        ClearDb();

        //
        UpdateUserRequest request = new("New Username");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }
}