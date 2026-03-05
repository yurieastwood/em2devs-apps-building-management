namespace EM2Devs.BuildingManagement.Application.Contracts.Residents;

public sealed record RegisterResidentRequest(
    Guid BuildingId,
    Guid UnitId,
    string FullName,
    string Email,
    string Phone,
    string DocumentNumber,
    string Role
);
