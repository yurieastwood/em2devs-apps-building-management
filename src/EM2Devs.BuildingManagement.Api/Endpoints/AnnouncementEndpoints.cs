using EM2Devs.BuildingManagement.Application.Contracts.Announcements;
using EM2Devs.BuildingManagement.Application.Contracts.Common;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class AnnouncementEndpoints
{
    public static IEndpointRouteBuilder MapAnnouncementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/announcements")
            .WithTags("Announcements")
            .RequireAuthorization();

        group.MapPost("/", (CreateAnnouncementRequest request) =>
        {
            var response = new AnnouncementResponse(Guid.NewGuid(), Guid.NewGuid(), request.Title, request.Body, "Draft", request.Audience, null, null, DateTime.UtcNow);
            return Results.Created($"/announcements/{response.AnnouncementId}", response);
        })
        .WithName("CreateAnnouncement")
        .Produces<AnnouncementResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<AnnouncementResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListAnnouncements")
        .Produces<PagedResponse<AnnouncementResponse>>();

        group.MapGet("/{announcementId:guid}", (Guid announcementId) =>
        {
            var audience = new AudienceSpecificationDto("BuildingWide", Guid.NewGuid(), null, null);
            var response = new AnnouncementResponse(announcementId, Guid.NewGuid(), "Stub Announcement", "Stub body", "Draft", audience, null, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetAnnouncement")
        .Produces<AnnouncementResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{announcementId:guid}", (Guid announcementId, UpdateAnnouncementRequest request) =>
        {
            var response = new AnnouncementResponse(announcementId, Guid.NewGuid(), request.Title, request.Body, "Draft", request.Audience, null, null, DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("UpdateAnnouncement")
        .Produces<AnnouncementResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
        .ProducesValidationProblem();

        group.MapPost("/{announcementId:guid}/publish", (Guid announcementId) =>
        {
            return Results.NoContent();
        })
        .WithName("PublishAnnouncement")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/{announcementId:guid}/archive", (Guid announcementId) =>
        {
            return Results.NoContent();
        })
        .WithName("ArchiveAnnouncement")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        group.MapDelete("/{announcementId:guid}", (Guid announcementId) =>
        {
            return Results.NoContent();
        })
        .WithName("DeleteDraftAnnouncement")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }
}
