namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record UpdateUnitRequest(
    string UnitNumber,
    int Floor,
    string Type
);
