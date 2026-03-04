namespace EM2Devs.BuildingManagement.Application.Contracts.Documents;

public sealed record UploadDocumentRequest(
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes
);
