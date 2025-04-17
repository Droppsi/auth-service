namespace AuthService.Contract.Responses.Users;

public record UsersResponse(IEnumerable<UserResponse> Users);