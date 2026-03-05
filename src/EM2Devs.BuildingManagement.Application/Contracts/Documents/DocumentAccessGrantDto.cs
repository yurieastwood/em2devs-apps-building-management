namespace EM2Devs.BuildingManagement.Application.Contracts.Documents;

public sealed record DocumentAccessGrantDto(
    Guid GrantId,
    string GranteeType,
    Guid GranteeId,
    Guid GrantedByManagerId,
    DateTime GrantedAt,
    DateTime? RevokedAt
);
