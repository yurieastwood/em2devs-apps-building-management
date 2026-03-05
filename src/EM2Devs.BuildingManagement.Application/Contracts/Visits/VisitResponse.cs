namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record VisitResponse(
    Guid VisitId,
    Guid BuildingId,
    Guid ManagerId,
    string Status,
    DateTime ScheduledDate,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Notes,
    IReadOnlyList<ChecklistItemResponse> ChecklistItems,
    IReadOnlyList<FollowUpActionResponse> FollowUpActions,
    DateTime CreatedAt
);
