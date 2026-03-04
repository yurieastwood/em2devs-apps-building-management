namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record ResolveChecklistItemRequest(
    string Status,
    string? Notes
);
