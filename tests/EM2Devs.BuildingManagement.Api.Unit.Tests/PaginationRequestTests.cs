using EM2Devs.BuildingManagement.Application.Contracts.Common;

namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class PaginationRequestTests
{
    [Fact]
    public void Defaults_AreApplied()
    {
        var request = new PaginationRequest();

        Assert.Equal(1, request.Page);
        Assert.Equal(20, request.PageSize);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-100, 1)]
    public void Page_ClampedToMinimum(int input, int expected)
    {
        var request = new PaginationRequest(Page: input);

        Assert.Equal(expected, request.Page);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(-100, 1)]
    public void PageSize_ClampedToMinimum(int input, int expected)
    {
        var request = new PaginationRequest(PageSize: input);

        Assert.Equal(expected, request.PageSize);
    }

    [Theory]
    [InlineData(101, 100)]
    [InlineData(500, 100)]
    public void PageSize_ClampedToMaximum(int input, int expected)
    {
        var request = new PaginationRequest(PageSize: input);

        Assert.Equal(expected, request.PageSize);
    }

    [Fact]
    public void ValidValues_ArePreserved()
    {
        var request = new PaginationRequest(Page: 3, PageSize: 50);

        Assert.Equal(3, request.Page);
        Assert.Equal(50, request.PageSize);
    }
}
