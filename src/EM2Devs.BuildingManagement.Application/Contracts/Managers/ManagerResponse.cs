namespace EM2Devs.BuildingManagement.Application.Contracts.Managers;

public sealed record ManagerResponse(
    Guid ManagerId,
    string Email,
    string FullName,
    string Role,
    IReadOnlyList<Guid> AssignedBuildingIds,
    DateTime CreatedAt
);
