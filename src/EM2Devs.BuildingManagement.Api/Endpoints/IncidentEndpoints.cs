using EM2Devs.BuildingManagement.Application.Contracts.Common;
using EM2Devs.BuildingManagement.Application.Contracts.Incidents;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class IncidentEndpoints
{
    public static IEndpointRouteBuilder MapIncidentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/incidents")
            .WithTags("Incidents")
            .RequireAuthorization();

        group.MapPost("/", (ReportIncidentRequest request) =>
        {
            var response = new IncidentResponse(Guid.NewGuid(), request.BuildingId, request.UnitId, Guid.NewGuid(), request.Title, request.Description, request.Type, request.Severity, "Open", request.Location, DateTime.UtcNow, null, null, null, DateTime.UtcNow);
            return Results.Created($"/incidents/{response.IncidentId}", response);
        })
        .WithName("ReportIncident")
        .Produces<IncidentResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<IncidentResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListIncidents")
        .Produces<PagedResponse<IncidentResponse>>();

        group.MapGet("/{incidentId:guid}", (Guid incidentId) =>
        {
            var response = new IncidentResponse(incidentId, Guid.NewGuid(), null, Guid.NewGuid(), "Stub Incident", "Stub description", "Maintenance", "Medium", "Open", null, DateTime.UtcNow, null, null, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetIncident")
        .Produces<IncidentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{incidentId:guid}/status", (Guid incidentId, UpdateIncidentStatusRequest request) =>
        {
            var response = new IncidentResponse(incidentId, Guid.NewGuid(), null, Guid.NewGuid(), "Stub Incident", "Stub description", "Maintenance", "Medium", request.Status, null, DateTime.UtcNow, DateTime.UtcNow, null, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("UpdateIncidentStatus")
        .Produces<IncidentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{incidentId:guid}/resolution-notes", (Guid incidentId, AddResolutionNotesRequest request) =>
        {
            var response = new IncidentResponse(incidentId, Guid.NewGuid(), null, Guid.NewGuid(), "Stub Incident", "Stub description", "Maintenance", "Medium", "Resolved", null, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, request.ResolutionNotes, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("AddResolutionNotes")
        .Produces<IncidentResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
