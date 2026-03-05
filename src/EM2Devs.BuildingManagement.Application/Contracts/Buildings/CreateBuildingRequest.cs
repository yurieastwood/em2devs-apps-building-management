namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record CreateBuildingRequest(
    string Name,
    AddressDto Address,
    int TotalFloors
);
