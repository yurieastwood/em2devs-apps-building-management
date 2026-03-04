using EM2Devs.BuildingManagement.Application.Contracts.Common;
using EM2Devs.BuildingManagement.Application.Contracts.Residents;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class ResidentEndpoints
{
    public static IEndpointRouteBuilder MapResidentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/residents")
            .WithTags("Residents")
            .RequireAuthorization();

        group.MapPost("/", (RegisterResidentRequest request) =>
        {
            var response = new ResidentResponse(Guid.NewGuid(), request.BuildingId, request.UnitId, request.FullName, request.Email, request.Phone, request.DocumentNumber, request.Role, "Invited", DateTime.UtcNow, null, DateTime.UtcNow);
            return Results.Created($"/residents/{response.ResidentId}", response);
        })
        .WithName("RegisterResident")
        .Produces<ResidentResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .Produces<ErrorResponse>(StatusCodes.Status409Conflict);

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<ResidentResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListResidents")
        .Produces<PagedResponse<ResidentResponse>>();

        group.MapGet("/{residentId:guid}", (Guid residentId) =>
        {
            var response = new ResidentResponse(residentId, Guid.NewGuid(), Guid.NewGuid(), "Stub Resident", "stub@example.com", "+5511999999999", "000.000.000-00", "Owner", "Active", DateTime.UtcNow, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetResident")
        .Produces<ResidentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{residentId:guid}", (Guid residentId, UpdateResidentInfoRequest request) =>
        {
            var response = new ResidentResponse(residentId, Guid.NewGuid(), Guid.NewGuid(), request.FullName, request.Email, request.Phone, "000.000.000-00", "Owner", "Active", DateTime.UtcNow, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("UpdateResidentInfo")
        .Produces<ResidentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapPost("/{residentId:guid}/invite", (Guid residentId) =>
        {
            return Results.NoContent();
        })
        .WithName("InviteResident")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{residentId:guid}/move-out", (Guid residentId) =>
        {
            return Results.NoContent();
        })
        .WithName("MoveOutResident")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{residentId:guid}/personal-data", (Guid residentId) =>
        {
            return Results.NoContent();
        })
        .WithName("EraseResidentPersonalData")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
