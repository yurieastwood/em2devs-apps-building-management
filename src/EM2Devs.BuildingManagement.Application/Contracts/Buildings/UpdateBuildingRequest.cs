namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record UpdateBuildingRequest(
    string Name,
    AddressDto Address,
    int TotalFloors
);
