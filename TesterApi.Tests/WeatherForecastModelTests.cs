using Xunit;
using GeoTesterApi;

namespace GeoTesterApi.Tests;

public class WeatherForecastModelTests
{
    [Theory]
    [InlineData(0, 32)]
    [InlineData(100, 211)]
    [InlineData(-40, -39)]
    [InlineData(37, 98)]
    public void TemperatureF_ConvertsFromCelsius(int celsius, int expectedFahrenheit)
    {
        var forecast = new WeatherForecast { TemperatureC = celsius };

        Assert.Equal(expectedFahrenheit, forecast.TemperatureF);
    }

    [Fact]
    public void WeatherForecast_CanSetAndGetProperties()
    {
        var date = new DateOnly(2026, 4, 15);
        var forecast = new WeatherForecast
        {
            Date = date,
            TemperatureC = 20,
            Summary = "Mild"
        };

        Assert.Equal(date, forecast.Date);
        Assert.Equal(20, forecast.TemperatureC);
        Assert.Equal("Mild", forecast.Summary);
    }
}
