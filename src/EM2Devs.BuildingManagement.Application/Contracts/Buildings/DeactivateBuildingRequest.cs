namespace EM2Devs.BuildingManagement.Application.Contracts.Buildings;

public sealed record DeactivateBuildingRequest(
    bool Force = false
);
