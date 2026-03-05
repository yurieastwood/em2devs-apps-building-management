namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record AddUnitRequest(
    string UnitNumber,
    int Floor,
    string Type
);
