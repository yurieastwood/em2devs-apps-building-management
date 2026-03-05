namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record BuildingResponse(
    Guid BuildingId,
    string Name,
    AddressDto Address,
    int TotalFloors,
    DateTime CreatedAt
);
