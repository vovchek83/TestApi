using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using GeoTesterApi;
using Xunit;

namespace GeoTesterApi.Tests;

public class WeatherForecastControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastControllerTests(WebApplicationFactory<Program> factory)
    {
        var baseDirectory = AppContext.BaseDirectory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsOk()
    {
        var response = await _client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsJsonContentType()
    {
        var response = await _client.GetAsync("/WeatherForecast");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_ReturnsFiveForecasts()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts.Length);
    }

    [Fact]
    public async Task Get_ForecastsHaveValidTemperatureRange()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(forecasts);
        foreach (var forecast in forecasts)
        {
            Assert.InRange(forecast.TemperatureC, -20, 55);
        }
    }

    [Fact]
    public async Task Get_ForecastsHaveValidDates()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(forecasts);
        var today = DateOnly.FromDateTime(DateTime.Now);
        foreach (var forecast in forecasts)
        {
            Assert.True(forecast.Date > today, $"Expected date in the future but got {forecast.Date}");
        }
    }

    [Fact]
    public async Task Get_ForecastsHaveNonNullSummaries()
    {
        var forecasts = await _client.GetFromJsonAsync<WeatherForecast[]>("/WeatherForecast");

        Assert.NotNull(forecasts);
        Assert.All(forecasts, f => Assert.NotNull(f.Summary));
    }
}
