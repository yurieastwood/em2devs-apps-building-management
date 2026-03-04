namespace EM2Devs.BuildingManagement.Application.Contracts.Documents;

public sealed record DocumentResponse(
    Guid DocumentId,
    Guid UploadedByManagerId,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    IReadOnlyList<DocumentAccessGrantDto> AccessGrants,
    DateTime CreatedAt
);
