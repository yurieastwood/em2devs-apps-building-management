using System.Net;
using System.Net.Http.Json;
using EM2Devs.BuildingManagement.Application.Contracts.Announcements;
using EM2Devs.BuildingManagement.Application.Contracts.Authentication;
using EM2Devs.BuildingManagement.Application.Contracts.Buildings;
using EM2Devs.BuildingManagement.Application.Contracts.Documents;
using EM2Devs.BuildingManagement.Application.Contracts.Incidents;
using EM2Devs.BuildingManagement.Application.Contracts.Managers;
using EM2Devs.BuildingManagement.Application.Contracts.Residents;
using EM2Devs.BuildingManagement.Application.Contracts.Visits;

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
        var response = await _client.PostAsJsonAsync("/auth/register",
            new RegisterRequest("test@example.com", "Test User", "Test123!", "Test123!"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AuthLogin_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/auth/login",
            new LoginRequest("test@example.com", "Test123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthRefresh_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/auth/refresh",
            new RefreshRequest("stub-token"));

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
        var address = new AddressDto("Test St", "1", null, "Centro", "São Paulo", "SP", "01000-000", "BR");
        var response = await _client.PostAsJsonAsync("/buildings",
            new CreateBuildingRequest("Test Building", address, 10));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_ReturnsOk()
    {
        var address = new AddressDto("Test St", "1", null, "Centro", "São Paulo", "SP", "01000-000", "BR");
        var response = await _client.PutAsJsonAsync($"/buildings/{Guid.NewGuid()}",
            new UpdateBuildingRequest("Updated Building", address, 15));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddUnit_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync($"/buildings/{Guid.NewGuid()}/units",
            new AddUnitRequest("101", 1, "Residential"));

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
    public async Task CreateManager_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/managers",
            new CreateManagerRequest("manager@example.com", "Test Manager", "Test123!", "BuildingManager"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetManager_ReturnsOk()
    {
        var response = await _client.GetAsync($"/managers/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListResidents_ReturnsOk()
    {
        var response = await _client.GetAsync("/residents?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegisterResident_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/residents",
            new RegisterResidentRequest(Guid.NewGuid(), Guid.NewGuid(), "Test Resident", "resident@example.com", "+5511999999999", "123.456.789-00", "Owner"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetResident_ReturnsOk()
    {
        var response = await _client.GetAsync($"/residents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListAnnouncements_ReturnsOk()
    {
        var response = await _client.GetAsync("/announcements?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAnnouncement_ReturnsCreated()
    {
        var audience = new AudienceSpecificationDto("BuildingWide", Guid.NewGuid(), null, null);
        var response = await _client.PostAsJsonAsync("/announcements",
            new CreateAnnouncementRequest("Test Announcement", "Test body", audience));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetAnnouncement_ReturnsOk()
    {
        var response = await _client.GetAsync($"/announcements/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListDocuments_ReturnsOk()
    {
        var response = await _client.GetAsync("/documents?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadDocument_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/documents/upload",
            new UploadDocumentRequest("test.pdf", "application/pdf", 1024));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetDocument_ReturnsOk()
    {
        var response = await _client.GetAsync($"/documents/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListVisits_ReturnsOk()
    {
        var response = await _client.GetAsync("/visits?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ScheduleVisit_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/visits",
            new ScheduleVisitRequest(Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetVisit_ReturnsOk()
    {
        var response = await _client.GetAsync($"/visits/{Guid.NewGuid()}");

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
        var response = await _client.PostAsJsonAsync("/incidents",
            new ReportIncidentRequest(Guid.NewGuid(), null, "Test Incident", "A test incident", "Maintenance", "Medium", null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
        var response = await _client.PatchAsJsonAsync($"/incidents/{Guid.NewGuid()}/status",
            new UpdateIncidentStatusRequest("InProgress"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
