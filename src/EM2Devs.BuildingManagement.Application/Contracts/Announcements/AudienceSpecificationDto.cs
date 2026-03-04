namespace EM2Devs.BuildingManagement.Application.Contracts.Announcements;

public sealed record AudienceSpecificationDto(
    string Scope,
    Guid BuildingId,
    IReadOnlyList<Guid>? UnitIds,
    IReadOnlyList<Guid>? ResidentIds
);
