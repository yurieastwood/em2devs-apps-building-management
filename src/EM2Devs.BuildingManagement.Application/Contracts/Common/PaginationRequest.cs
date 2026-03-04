namespace EM2Devs.BuildingManagement.Application.Contracts.Common;

public sealed record PaginationRequest(
    int Page = 1,
    int PageSize = 20
);
