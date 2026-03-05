namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record UnitResponse(
    Guid UnitId,
    Guid BuildingId,
    string UnitNumber,
    int Floor,
    string Type,
    DateTime CreatedAt
);
