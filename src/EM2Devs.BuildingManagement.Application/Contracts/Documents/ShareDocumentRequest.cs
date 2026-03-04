namespace EM2Devs.BuildingManagement.Application.Contracts.Documents;

public sealed record ShareDocumentRequest(
    string GranteeType,
    Guid GranteeId
);
