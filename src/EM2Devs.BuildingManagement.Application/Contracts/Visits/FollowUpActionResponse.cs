namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record FollowUpActionResponse(
    Guid FollowUpActionId,
    string Description,
    string Status,
    Guid? LinkedIncidentId,
    DateTime? DueDate,
    DateTime? CompletedAt
);
