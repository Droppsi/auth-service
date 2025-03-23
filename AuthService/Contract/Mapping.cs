using AuthService.Contract.Responses.Users;
using AuthService.Models;

namespace AuthService.Contract;

public static class Mapping
{
    public static UserResponse MapToResponse(this User user)
    {
        return new UserResponse(user.Id, user.Username);
    }

    public static UsersResponse MapToResponse(this IEnumerable<User> users)
    {
        return new UsersResponse(users.Select(x => x.MapToResponse()));
    }
}