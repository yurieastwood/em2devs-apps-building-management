using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/weatherforecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveForecasts()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecastResponse[]>("/weatherforecast");

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsValidData()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecastResponse[]>("/weatherforecast");

        Assert.NotNull(forecasts);
        Assert.All(forecasts, forecast =>
        {
            Assert.NotNull(forecast.Summary);
            Assert.NotEqual(default, forecast.Date);
        });
    }

    private record WeatherForecastResponse(DateOnly Date, int TemperatureC, int TemperatureF, string? Summary);
}
