namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record AddChecklistItemRequest(
    string Category,
    string Description
);
