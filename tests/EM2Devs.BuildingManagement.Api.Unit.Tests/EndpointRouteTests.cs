using System.Net;
using System.Net.Http.Json;

namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class EndpointRouteTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EndpointRouteTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AuthRegister_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            Email = "test@example.com",
            FullName = "Test User",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AuthLogin_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            Email = "test@example.com",
            Password = "Test123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthRefresh_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh", new
        {
            RefreshToken = "stub-token"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthLogout_ReturnsNoContent()
    {
        var response = await _client.PostAsync("/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ListBuildings_ReturnsOk()
    {
        var response = await _client.GetAsync("/buildings?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBuilding_ReturnsOk()
    {
        var response = await _client.GetAsync($"/buildings/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/buildings", new
        {
            Name = "Test Building",
            Address = new
            {
                Street = "Test St",
                Number = "1",
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP",
                PostalCode = "01000-000",
                Country = "BR"
            },
            TotalFloors = 10
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ListResidents_ReturnsOk()
    {
        var response = await _client.GetAsync("/residents?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListAnnouncements_ReturnsOk()
    {
        var response = await _client.GetAsync("/announcements?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListDocuments_ReturnsOk()
    {
        var response = await _client.GetAsync("/documents?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListVisits_ReturnsOk()
    {
        var response = await _client.GetAsync("/visits?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListIncidents_ReturnsOk()
    {
        var response = await _client.GetAsync("/incidents?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReportIncident_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/incidents", new
        {
            BuildingId = Guid.NewGuid(),
            Title = "Test Incident",
            Description = "A test incident",
            Type = "Maintenance",
            Severity = "Medium"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateManager_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/managers", new
        {
            Email = "manager@example.com",
            FullName = "Test Manager",
            Password = "Test123!",
            Role = "BuildingManager"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetManager_ReturnsOk()
    {
        var response = await _client.GetAsync($"/managers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_ReturnsOk()
    {
        var response = await _client.PutAsJsonAsync($"/buildings/{Guid.NewGuid()}", new
        {
            Name = "Updated Building",
            Address = new { Street = "Test St", Number = "1", Neighborhood = "Centro", City = "São Paulo", State = "SP", PostalCode = "01000-000", Country = "BR" },
            TotalFloors = 15
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddUnit_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync($"/buildings/{Guid.NewGuid()}/units", new
        {
            UnitNumber = "101",
            Floor = 1,
            Type = "Residential"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ListUnits_ReturnsOk()
    {
        var response = await _client.GetAsync($"/buildings/{Guid.NewGuid()}/units?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUnit_ReturnsOk()
    {
        var response = await _client.GetAsync($"/buildings/{Guid.NewGuid()}/units/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterResident_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/residents", new
        {
            BuildingId = Guid.NewGuid(),
            UnitId = Guid.NewGuid(),
            FullName = "Test Resident",
            Email = "resident@example.com",
            Phone = "+5511999999999",
            DocumentNumber = "123.456.789-00",
            Role = "Owner"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetResident_ReturnsOk()
    {
        var response = await _client.GetAsync($"/residents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAnnouncement_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/announcements", new
        {
            Title = "Test Announcement",
            Body = "Test body",
            Audience = new { Scope = "BuildingWide", BuildingId = Guid.NewGuid() }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAnnouncement_ReturnsOk()
    {
        var response = await _client.GetAsync($"/announcements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadDocument_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/documents/upload", new
        {
            OriginalFileName = "test.pdf",
            ContentType = "application/pdf",
            FileSizeBytes = 1024
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetDocument_ReturnsOk()
    {
        var response = await _client.GetAsync($"/documents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleVisit_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/visits", new
        {
            BuildingId = Guid.NewGuid(),
            ScheduledDate = DateTime.UtcNow.AddDays(1)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetVisit_ReturnsOk()
    {
        var response = await _client.GetAsync($"/visits/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetIncident_ReturnsOk()
    {
        var response = await _client.GetAsync($"/incidents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateIncidentStatus_ReturnsOk()
    {
        var response = await _client.PatchAsJsonAsync($"/incidents/{Guid.NewGuid()}/status", new
        {
            Status = "InProgress"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
