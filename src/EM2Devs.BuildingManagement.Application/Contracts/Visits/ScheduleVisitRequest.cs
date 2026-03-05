namespace EM2Devs.BuildingManagement.Application.Contracts.Visits;

public sealed record ScheduleVisitRequest(
    Guid BuildingId,
    DateTime ScheduledDate,
    string? Notes
);
