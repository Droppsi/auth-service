namespace AuthService.Contract.Responses.Users;

public record LoginResponse(string AccessToken, string RefreshToken);