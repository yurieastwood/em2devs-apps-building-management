namespace EM2Devs.BuildingManagement.Application.Contracts.Authentication;

public sealed record LoginRequest(
    string Email,
    string Password
);
