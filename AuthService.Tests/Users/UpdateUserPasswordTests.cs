using System.Net;
using System.Net.Http.Json;
using AuthService.Contract.Requests.Users;
using AuthService.Models;
using Shouldly;

namespace AuthService.Tests.Users;

public class UpdateUserPasswordTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Should_Return_200_OK_When_Changing_Password_Of_Existing_User()
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

        UpdateUserPasswordRequest request = new("New Password");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}/password", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        RefreshDbContext();
        User? updatedUser = DbContext.Users.SingleOrDefault(u => u.Id == user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.Username.ShouldBe("Username");
        updatedUser.Password.ShouldNotBe(user.Password);
        BCrypt.Net.BCrypt.EnhancedVerify(request.Password, updatedUser.Password)
            .ShouldBeTrue("Password should be hashed and verified");
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Changing_Password_Of_Existing_User_To_Empty_String()
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

        UpdateUserPasswordRequest request = new("");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}/password", request);

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
    public async Task Should_Return_404_NotFound_When_Changing_Password_Of_Non_Existing_User()
    {
        ClearDb();

        // Arrange
        UpdateUserPasswordRequest request = new("New Password");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/password", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Changing_Password_Of_Existing_User_To_Null()
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

        UpdateUserPasswordRequest request = new(null!);

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}/password", request);

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
    public async Task Should_Return_400_BadRequest_When_Changing_Password_Of_Existing_User_To_Empty_String_With_Whitespace()
    {
        ClearDb();

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "Username",
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        UpdateUserPasswordRequest request = new("   ");

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}/password", request);

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
    public async Task Should_Return_400_BadRequest_When_Password_Is_513_Letters_Long()
    {
        ClearDb();

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "Username",
        };
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();
        UpdateUserPasswordRequest request = new(new string('a', 513));

        // Act
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/api/users/{user.Id}/password", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        User? updatedUser = DbContext.Users.SingleOrDefault(u => u.Id == user.Id);
        updatedUser.ShouldNotBeNull();
        updatedUser.Username.ShouldBe("Username");
        updatedUser.Password.ShouldBe(user.Password);
        updatedUser.Id.ShouldBe(user.Id);
    }
}