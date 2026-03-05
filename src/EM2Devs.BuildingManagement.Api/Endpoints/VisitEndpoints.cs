using EM2Devs.BuildingManagement.Application.Contracts.Common;
using EM2Devs.BuildingManagement.Application.Contracts.Visits;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class VisitEndpoints
{
    public static IEndpointRouteBuilder MapVisitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/visits")
            .WithTags("Visits")
            .RequireAuthorization();

        group.MapPost("/", (ScheduleVisitRequest request) =>
        {
            var response = new VisitResponse(Guid.NewGuid(), request.BuildingId, Guid.NewGuid(), "Scheduled", request.ScheduledDate, null, null, request.Notes, [], [], DateTime.UtcNow);
            return Results.Created($"/visits/{response.VisitId}", response);
        })
        .WithName("ScheduleVisit")
        .Produces<VisitResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<VisitResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListVisits")
        .Produces<PagedResponse<VisitResponse>>();

        group.MapGet("/{visitId:guid}", (Guid visitId) =>
        {
            var response = new VisitResponse(visitId, Guid.NewGuid(), Guid.NewGuid(), "Scheduled", DateTime.UtcNow, null, null, null, [], [], DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetVisit")
        .Produces<VisitResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPatch("/{visitId:guid}/start", (Guid visitId) =>
        {
            return Results.NoContent();
        })
        .WithName("StartVisit")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{visitId:guid}/checklist-items", (Guid visitId, AddChecklistItemRequest request) =>
        {
            var response = new ChecklistItemResponse(Guid.NewGuid(), request.Category, request.Description, "Pending", null);
            return Results.Created($"/visits/{visitId}/checklist-items/{response.ChecklistItemId}", response);
        })
        .WithName("AddChecklistItem")
        .Produces<ChecklistItemResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
        .ProducesValidationProblem();

        group.MapPatch("/{visitId:guid}/checklist-items/{itemId:guid}", (Guid visitId, Guid itemId, ResolveChecklistItemRequest request) =>
        {
            var response = new ChecklistItemResponse(itemId, "Facilities", "Stub item", request.Status, request.Notes);
            return Results.Ok(response);
        })
        .WithName("ResolveChecklistItem")
        .Produces<ChecklistItemResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{visitId:guid}/follow-up-actions", (Guid visitId, AddFollowUpActionRequest request) =>
        {
            var response = new FollowUpActionResponse(Guid.NewGuid(), request.Description, "Open", null, request.DueDate, null);
            return Results.Created($"/visits/{visitId}/follow-up-actions/{response.FollowUpActionId}", response);
        })
        .WithName("AddFollowUpAction")
        .Produces<FollowUpActionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
        .ProducesValidationProblem();

        group.MapPatch("/{visitId:guid}/follow-up-actions/{actionId:guid}/link-incident", (Guid visitId, Guid actionId, LinkFollowUpToIncidentRequest request) =>
        {
            return Results.NoContent();
        })
        .WithName("LinkFollowUpToIncident")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{visitId:guid}/follow-up-actions/{actionId:guid}/complete", (Guid visitId, Guid actionId) =>
        {
            return Results.NoContent();
        })
        .WithName("CompleteFollowUpAction")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{visitId:guid}/complete", (Guid visitId) =>
        {
            return Results.NoContent();
        })
        .WithName("CompleteVisit")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
