using System.Net;
using System.Text.Json;

namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class OpenApiSpecTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OpenApiSpecTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task OpenApiSpec_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task OpenApiSpec_ContainsAllRoutePrefixes()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var paths = document.RootElement.GetProperty("paths");

        var expectedPrefixes = new[] { "/auth/", "/managers", "/buildings", "/residents", "/announcements", "/documents", "/visits", "/incidents" };

        foreach (var prefix in expectedPrefixes)
        {
            var hasPath = false;
            foreach (var path in paths.EnumerateObject())
            {
                if (path.Name.StartsWith(prefix))
                {
                    hasPath = true;
                    break;
                }
            }

            Assert.True(hasPath, $"OpenAPI spec should contain paths starting with '{prefix}'");
        }
    }

    [Fact]
    public async Task OpenApiSpec_ContainsAllEndpointTags()
    {
        var response = await _client.GetAsync("/openapi/v1.json");
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var tags = document.RootElement.GetProperty("tags");

        var expectedTags = new[] { "Authentication", "Managers", "Buildings", "Residents", "Announcements", "Documents", "Visits", "Incidents" };
        var actualTags = tags.EnumerateArray()
            .Select(t => t.GetProperty("name").GetString())
            .ToHashSet();

        foreach (var expected in expectedTags)
        {
            Assert.Contains(expected, actualTags);
        }
    }
}
