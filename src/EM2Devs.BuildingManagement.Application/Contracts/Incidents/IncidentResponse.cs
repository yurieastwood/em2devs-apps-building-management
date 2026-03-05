namespace EM2Devs.BuildingManagement.Application.Contracts.Incidents;

public sealed record IncidentResponse(
    Guid IncidentId,
    Guid BuildingId,
    Guid? UnitId,
    Guid ReportedByManagerId,
    string Title,
    string Description,
    string Type,
    string Severity,
    string Status,
    string? Location,
    DateTime OpenedAt,
    DateTime? InProgressAt,
    DateTime? ResolvedAt,
    string? ResolutionNotes,
    DateTime CreatedAt
);
