using EM2Devs.BuildingManagement.Application.Contracts.Common;
using EM2Devs.BuildingManagement.Application.Contracts.Documents;

namespace EM2Devs.BuildingManagement.Api.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/documents")
            .WithTags("Documents")
            .RequireAuthorization();

        group.MapPost("/upload", (UploadDocumentRequest request) =>
        {
            var response = new DocumentResponse(Guid.NewGuid(), Guid.NewGuid(), request.OriginalFileName, request.ContentType, request.FileSizeBytes, [], DateTime.UtcNow);
            return Results.Created($"/documents/{response.DocumentId}", response);
        })
        .WithName("UploadDocument")
        .Produces<DocumentResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/", ([AsParameters] PaginationRequest pagination) =>
        {
            var response = new PagedResponse<DocumentResponse>([], pagination.Page, pagination.PageSize, 0);
            return Results.Ok(response);
        })
        .WithName("ListDocuments")
        .Produces<PagedResponse<DocumentResponse>>();

        group.MapGet("/{documentId:guid}", (Guid documentId) =>
        {
            var response = new DocumentResponse(documentId, Guid.NewGuid(), "stub-file.pdf", "application/pdf", 1024, [], DateTime.UtcNow);
            return Results.Ok(response);
        })
        .WithName("GetDocument")
        .Produces<DocumentResponse>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{documentId:guid}/share", (Guid documentId, ShareDocumentRequest request) =>
        {
            return Results.NoContent();
        })
        .WithName("ShareDocument")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();

        group.MapDelete("/{documentId:guid}/access/{grantId:guid}", (Guid documentId, Guid grantId) =>
        {
            return Results.NoContent();
        })
        .WithName("RevokeDocumentAccess")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{documentId:guid}", (Guid documentId) =>
        {
            return Results.NoContent();
        })
        .WithName("DeleteDocument")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
