using EM2Devs.BuildingManagement.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapAuthenticationEndpoints();
app.MapManagerEndpoints();
app.MapBuildingEndpoints();
app.MapResidentEndpoints();
app.MapAnnouncementEndpoints();
app.MapDocumentEndpoints();
app.MapVisitEndpoints();
app.MapIncidentEndpoints();

app.Run();
