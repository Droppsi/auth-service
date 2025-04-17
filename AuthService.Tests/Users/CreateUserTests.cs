using System.Net;
using System.Net.Http.Json;
using System.Text;
using AuthService.Contract.Requests.Users;
using AuthService.Models;
using Bogus;
using Shouldly;

namespace AuthService.Tests.Users;

public class CreateUserTests(CustomWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly Faker<CreateUserRequest> _fakeUserGenerator = new Faker<CreateUserRequest>()
        .CustomInstantiator(f => new CreateUserRequest(
            f.Internet.UserName(),
            f.Internet.Password(8)
        ));

    [Fact]
    public async Task Should_Return_200_OK_When_Creating_User()
    {
        ClearDb();

        // Arrange
        CreateUserRequest? request = _fakeUserGenerator.Generate();
        request.ShouldNotBeNull();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        RefreshDbContext();
        User? user = DbContext.Users.SingleOrDefault(u => u.Username == request.Username);

        user.ShouldNotBeNull();
        user.Username.ShouldBe(request.Username);
        user.Password.ShouldNotBeNullOrEmpty();
        user.Password.ShouldNotBe(request.Password);
        BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password)
            .ShouldBeTrue("Password should be hashed and verified");
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Empty_Username()
    {
        ClearDb();

        // Arrange
        var request = new CreateUserRequest("", "Password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Null_Username()
    {
        ClearDb();

        // Arrange
        var request = new CreateUserRequest(null!, "Password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Empty_Password()
    {
        ClearDb();

        // Arrange
        var request = new CreateUserRequest("Username", "");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Null_Password()
    {
        ClearDb();

        // Arrange
        var request = new CreateUserRequest("Username", null!);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Duplicate_Username()
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

        var request = new CreateUserRequest("Username", "Password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Username_Longer_Than_255_Letters()
    {
        ClearDb();

        // Arrange
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < 256; i++)
        {
            stringBuilder.Append('a');
        }

        var request = new CreateUserRequest(stringBuilder.ToString(), "Password");

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Return_400_BadRequest_When_Creating_User_With_Password_Longer_Than_512_Letters()
    {
        ClearDb();

        // Arrange
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < 513; i++)
        {
            stringBuilder.Append('a');
        }

        var request = new CreateUserRequest("Username", stringBuilder.ToString());

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        RefreshDbContext();
        DbContext.Users.ShouldNotBeNull().ShouldBeEmpty();
    }
}