namespace EM2Devs.BuildingManagement.Application.Contracts.Managers;

public sealed record CreateManagerRequest(
    string Email,
    string FullName,
    string Password,
    string Role
);
