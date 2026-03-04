using EM2Devs.BuildingManagement.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
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
