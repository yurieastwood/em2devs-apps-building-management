using System.Text;
using EM2Devs.BuildingManagement.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

if (!builder.Environment.IsDevelopment() && (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience)))
    throw new InvalidOperationException("Jwt:Key, Jwt:Issuer, and Jwt:Audience must be configured in non-Development environments.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer ?? "https://localhost",
            ValidAudience = jwtAudience ?? "building-management-api",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey ?? "dev-only-key-do-not-use-in-production!!"))
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
