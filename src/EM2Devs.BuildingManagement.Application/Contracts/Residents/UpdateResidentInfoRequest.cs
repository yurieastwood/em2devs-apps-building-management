namespace EM2Devs.BuildingManagement.Application.Contracts.Residents;

public sealed record UpdateResidentInfoRequest(
    string FullName,
    string Email,
    string Phone
);
