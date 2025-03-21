using AuthService.Contract.Requests.Users;
using AuthService.Infrastructure.Database;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/users", async (CreateUserRequest request, [FromServices] AppDbContext dbContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return Results.BadRequest();
            }

            if (request.Username.Length > 255)
            {
                return Results.BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest();
            }

            if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
            {
                return Results.BadRequest();
            }

            var user = new User
            {
                Username = request.Username,
                Id = Guid.NewGuid()
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            return Results.Ok(user);
        });
    }
}