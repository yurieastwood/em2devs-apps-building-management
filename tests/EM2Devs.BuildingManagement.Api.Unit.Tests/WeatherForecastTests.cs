namespace EM2Devs.BuildingManagement.Api.Unit.Tests;

public class WeatherForecastTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(25, 76)]
    [InlineData(-20, -3)]
    [InlineData(55, 130)]
    public void TemperatureF_ShouldConvertFromCelsius(int temperatureC, int expectedF)
    {
        var forecast = new WeatherForecast(DateOnly.FromDateTime(DateTime.Now), temperatureC, "Test");

        Assert.Equal(expectedF, forecast.TemperatureF);
    }
}
