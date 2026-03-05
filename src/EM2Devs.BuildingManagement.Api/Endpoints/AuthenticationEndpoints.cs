using EM2Devs.BuildingManagement.Application.Contracts.Authentication;
using EM2Devs.BuildingManagement.Application.Contracts.Common;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapPost("/register", (RegisterRequest request) =>
        {
            var response = new AuthResponse("stub-access-token", "stub-refresh-token", DateTime.UtcNow.AddMinutes(15));
            return Results.Created("/auth/register", response);
        })
        .WithName("Register")
        .Produces<AuthResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapPost("/login", (LoginRequest request) =>
        {
            var response = new AuthResponse("stub-access-token", "stub-refresh-token", DateTime.UtcNow.AddMinutes(15));
            return Results.Ok(response);
        })
        .WithName("Login")
        .Produces<AuthResponse>()
        .ProducesValidationProblem()
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", (RefreshRequest request) =>
        {
            var response = new AuthResponse("stub-access-token", "stub-refresh-token", DateTime.UtcNow.AddMinutes(15));
            return Results.Ok(response);
        })
        .WithName("RefreshToken")
        .Produces<AuthResponse>()
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", () =>
        {
            return Results.NoContent();
        })
        .WithName("Logout")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status401Unauthorized);

        return app;
    }
}
