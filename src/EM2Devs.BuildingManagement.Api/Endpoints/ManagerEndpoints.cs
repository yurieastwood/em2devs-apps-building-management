using EM2Devs.BuildingManagement.Application.Contracts.Common;
using EM2Devs.BuildingManagement.Application.Contracts.Managers;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class ManagerEndpoints
{
    public static IEndpointRouteBuilder MapManagerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/managers")
            .WithTags("Managers")
            .RequireAuthorization();

        group.MapPost("/", (CreateManagerRequest request) =>
        {
            var response = new ManagerResponse(Guid.NewGuid(), request.Email, request.FullName, request.Role, [], DateTime.UtcNow);
            return Results.Created($"/managers/{response.ManagerId}", response);
        })
        .WithName("CreateManager")
        .Produces<ManagerResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/{managerId:guid}", (Guid managerId) =>
        {
            var response = new ManagerResponse(managerId, "stub@example.com", "Stub Manager", "BuildingManager", [], DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetManager")
        .Produces<ManagerResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{managerId:guid}/buildings/{buildingId:guid}/assign", (Guid managerId, Guid buildingId) =>
        {
            return Results.NoContent();
        })
        .WithName("AssignBuildingToManager")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{managerId:guid}/buildings/{buildingId:guid}/revoke", (Guid managerId, Guid buildingId) =>
        {
            return Results.NoContent();
        })
        .WithName("RevokeBuildingFromManager")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{managerId:guid}/deactivate", (Guid managerId) =>
        {
            return Results.NoContent();
        })
        .WithName("DeactivateManager")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
