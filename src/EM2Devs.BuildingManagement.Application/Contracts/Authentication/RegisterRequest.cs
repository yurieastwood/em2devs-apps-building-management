namespace EM2Devs.BuildingManagement.Application.Contracts.Authentication;

public sealed record RegisterRequest(
    string Email,
    string FullName,
    string Password,
    string ConfirmPassword
);
