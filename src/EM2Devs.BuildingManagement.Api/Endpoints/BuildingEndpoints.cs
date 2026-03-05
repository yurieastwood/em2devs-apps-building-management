using EM2Devs.BuildingManagement.Application.Contracts.Buildings;
using EM2Devs.BuildingManagement.Application.Contracts.Common;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class BuildingEndpoints
{
    public static IEndpointRouteBuilder MapBuildingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/buildings")
            .WithTags("Buildings")
            .RequireAuthorization();

        group.MapPost("/", (CreateBuildingRequest request) =>
        {
            var response = new BuildingResponse(Guid.NewGuid(), request.Name, request.Address, request.TotalFloors, DateTime.UtcNow);
            return Results.Created($"/buildings/{response.BuildingId}", response);
        })
        .WithName("CreateBuilding")
        .Produces<BuildingResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<BuildingResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListBuildings")
        .Produces<PagedResponse<BuildingResponse>>();

        group.MapGet("/{buildingId:guid}", (Guid buildingId) =>
        {
            var stub = new AddressDto("Stub St", "1", null, "Centro", "São Paulo", "SP", "01000-000", "BR");
            var response = new BuildingResponse(buildingId, "Stub Building", stub, 10, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetBuilding")
        .Produces<BuildingResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{buildingId:guid}", (Guid buildingId, UpdateBuildingRequest request) =>
        {
            var response = new BuildingResponse(buildingId, request.Name, request.Address, request.TotalFloors, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("UpdateBuilding")
        .Produces<BuildingResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapPost("/{buildingId:guid}/units", (Guid buildingId, AddUnitRequest request) =>
        {
            var response = new UnitResponse(Guid.NewGuid(), buildingId, request.UnitNumber, request.Floor, request.Type, DateTime.UtcNow);
            return Results.Created($"/buildings/{buildingId}/units/{response.UnitId}", response);
        })
        .WithName("AddUnit")
        .Produces<UnitResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapGet("/{buildingId:guid}/units", (Guid buildingId, [AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<UnitResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListUnits")
        .Produces<PagedResponse<UnitResponse>>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{buildingId:guid}/units/{unitId:guid}", (Guid buildingId, Guid unitId) =>
        {
            var response = new UnitResponse(unitId, buildingId, "101", 1, "Residential", DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetUnit")
        .Produces<UnitResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{buildingId:guid}/units/{unitId:guid}", (Guid buildingId, Guid unitId, UpdateUnitRequest request) =>
        {
            var response = new UnitResponse(unitId, buildingId, request.UnitNumber, request.Floor, request.Type, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("UpdateUnit")
        .Produces<UnitResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapPost("/{buildingId:guid}/deactivate", (Guid buildingId, DeactivateBuildingRequest request) =>
        {
            return Results.NoContent();
        })
        .WithName("DeactivateBuilding")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
