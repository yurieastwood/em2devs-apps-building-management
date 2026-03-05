namespace EM2Devs.BuildingManagement.Application.Contracts.Incidents;

public sealed record ReportIncidentRequest(
    Guid BuildingId,
    Guid? UnitId,
    string Title,
    string Description,
    string Type,
    string Severity,
    string? Location
);
