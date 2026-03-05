namespace EM2Devs.BuildingManagement.Application.Contracts.Residents;

public sealed record ResidentResponse(
    Guid ResidentId,
    Guid BuildingId,
    Guid UnitId,
    string FullName,
    string Email,
    string Phone,
    string DocumentNumber,
    string Role,
    string Status,
    DateTime MoveInDate,
    DateTime? MoveOutDate,
    DateTime CreatedAt
);
