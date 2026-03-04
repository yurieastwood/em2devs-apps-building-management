using System.Text;
using EM2Devs.BuildingManagement.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TODO: Replace with real configuration (issuer, audience, signing key) from appsettings / secrets.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "building-management-api",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "REPLACE-WITH-A-SECURE-KEY-OF-AT-LEAST-32-CHARS!"))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
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
