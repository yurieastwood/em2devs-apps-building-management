using EM2Devs.BuildingManagement.Application.Contracts.Common;

namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class PagedResponseTests
{
    [Fact]
    public void TotalPages_RoundsUp()
    {
        var response = new PagedResponse<string>([], Page: 1, PageSize: 10, TotalCount: 25);

        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void TotalPages_ExactDivision()
    {
        var response = new PagedResponse<string>([], Page: 1, PageSize: 10, TotalCount: 20);

        Assert.Equal(2, response.TotalPages);
    }

    [Fact]
    public void TotalPages_EmptyCollection()
    {
        var response = new PagedResponse<string>([], Page: 1, PageSize: 10, TotalCount: 0);

        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public void HasNextPage_TrueWhenMorePagesExist()
    {
        var response = new PagedResponse<string>([], Page: 1, PageSize: 10, TotalCount: 25);

        Assert.True(response.HasNextPage);
    }

    [Fact]
    public void HasNextPage_FalseOnLastPage()
    {
        var response = new PagedResponse<string>([], Page: 3, PageSize: 10, TotalCount: 25);

        Assert.False(response.HasNextPage);
    }

    [Fact]
    public void HasPreviousPage_FalseOnFirstPage()
    {
        var response = new PagedResponse<string>([], Page: 1, PageSize: 10, TotalCount: 25);

        Assert.False(response.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_TrueOnSubsequentPages()
    {
        var response = new PagedResponse<string>([], Page: 2, PageSize: 10, TotalCount: 25);

        Assert.True(response.HasPreviousPage);
    }
}
