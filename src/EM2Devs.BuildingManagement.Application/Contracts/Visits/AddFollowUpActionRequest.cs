namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record AddFollowUpActionRequest(
    string Description,
    DateTime? DueDate
);
