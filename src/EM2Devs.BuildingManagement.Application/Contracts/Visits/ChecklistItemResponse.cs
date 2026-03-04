namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record ChecklistItemResponse(
    Guid ChecklistItemId,
    string Category,
    string Description,
    string Status,
    string? Notes
);
