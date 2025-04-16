using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Contract;
using AuthService.Contract.Requests.Users;
using AuthService.Contract.Responses.Users;
using AuthService.Infrastructure.Database;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

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

            if (request.Password.Length > 512)
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
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                Id = Guid.NewGuid()
            };

            await dbContext.Users.AddAsync(user);
            await dbContext.SaveChangesAsync();
            return Results.Ok(user);
        });

        endpoints.MapDelete("/api/users/{id:guid}", async (Guid id, [FromServices] AppDbContext dbContext) =>
        {
            User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null)
            {
                return Results.NotFound();
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        });

        endpoints.MapGet("/api/users/", async ([FromServices] AppDbContext dbContext) =>
        {
            var users = await dbContext.Users.ToListAsync();

            return Results.Ok(users.MapToResponse());
        });

        endpoints.MapGet("/api/users/{id:guid}", async (Guid id, [FromServices] AppDbContext dbContext) =>
        {
            User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user is null
                ? Results.NotFound()
                : Results.Ok(user.MapToResponse());
        });

        endpoints.MapPut("/api/users/{id:guid}",
            async ([FromRoute] Guid id, UpdateUserRequest request, [FromServices] AppDbContext dbContext) =>
            {
                User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user is null)
                {
                    return Results.NotFound();
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return Results.BadRequest();
                }

                user.Username = request.Username;
                await dbContext.SaveChangesAsync();
                return Results.Ok(user.MapToResponse());
            }
        );

        endpoints.MapPut("/api/users/{id:guid}/password",
            async ([FromRoute] Guid id, UpdateUserPasswordRequest request, [FromServices] AppDbContext dbContext) =>
            {
                User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user is null)
                {
                    return Results.NotFound();
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest();
                }

                if (request.Password.Length > 512)
                {
                    return Results.BadRequest();
                }

                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
                await dbContext.SaveChangesAsync();
                return Results.Ok(user.MapToResponse());
            }
        );

        endpoints.MapPost("/api/users/login",
            async (LoginUserRequest request, [FromServices] AppDbContext dbContext,
                [FromServices] IConfiguration configuration) =>
            {
                User? user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
                if (user is null)
                {
                    return Results.NotFound();
                }

                if (!BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password))
                {
                    return Results.Unauthorized();
                }

                var configKey = configuration.GetValue<string>("Jwt:Key");

                if (string.IsNullOrWhiteSpace(configKey))
                {
                    return Results.InternalServerError();
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var audience = configuration.GetValue<string>("Jwt:Audience");
                var issuer = configuration.GetValue<string>("Jwt:Issuer");
                var expires = configuration.GetValue<int?>("Jwt:ExpiresInSeconds");

                if (string.IsNullOrWhiteSpace(audience))
                {
                    return Results.InternalServerError();
                }

                if (string.IsNullOrWhiteSpace(issuer))
                {
                    return Results.InternalServerError();
                }

                if (expires is null or <= 0)
                {
                    return Results.InternalServerError();
                }

                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddSeconds(expires.Value),
                    signingCredentials: credentials
                );

                string? tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                if (string.IsNullOrWhiteSpace(tokenString))
                {
                    return Results.InternalServerError();
                }

                var refreshToken = Guid.NewGuid().ToString();

                user.RefreshToken = refreshToken;
                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync();

                return Results.Ok(new LoginResponse(tokenString, refreshToken));
            }
        );
    }
}