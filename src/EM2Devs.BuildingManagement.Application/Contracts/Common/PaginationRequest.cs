namespace EM2Devs.BuildingManagement.Application.Contracts.Common;

public sealed record PaginationRequest
{
    public const int MinPage = 1;
    public const int MinPageSize = 1;
    public const int MaxPageSize = 100;
    public const int DefaultPageSize = 20;

    public int Page { get; } = MinPage;
    public int PageSize { get; } = DefaultPageSize;

    public PaginationRequest(int Page = MinPage, int PageSize = DefaultPageSize)
    {
        this.Page = Math.Max(Page, MinPage);
        this.PageSize = Math.Clamp(PageSize, MinPageSize, MaxPageSize);
    }
}
