using System.Net;
using System.Net.Http.Json;
using System.Text;
using AuthService.Contract.Requests.Users;
using AuthService.Models;
using Bogus;

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
        Assert.NotNull(request);

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        RefreshDbContext();
        User? user = DbContext.Users.SingleOrDefault(u => u.Username == request.Username);

        Assert.NotNull(user);
        Assert.Equal(request.Username, user.Username);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Empty(DbContext.Users);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Empty(DbContext.Users);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Empty(DbContext.Users);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Empty(DbContext.Users);
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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Single(DbContext.Users);
    }

    // test if username is longer than 255 letters

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
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        RefreshDbContext();
        Assert.Empty(DbContext.Users);
    }
}