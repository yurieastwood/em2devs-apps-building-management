namespace EM2Devs.BuildingManagement.Application.Contracts.Common;

public sealed record ErrorResponse(
    string Code,
    string Message
);
