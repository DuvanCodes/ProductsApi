using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using ProductsApi.Dtos;

namespace ProductsApi.Services;

public class WeatherService(HttpClient httpClient, IMemoryCache cache) : IWeatherService
{
    private static readonly Dictionary<string, (double Lat, double Lon)> Cities = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cali"] = (3.4516, -76.5320),
        ["bogota"] = (4.7110, -74.0721),
        ["medellin"] = (6.2442, -75.5812),
        ["barranquilla"] = (10.9685, -74.7813),
        ["cartagena"] = (10.3910, -75.4794)
    };

    public IEnumerable<string> SupportedCities => Cities.Keys;

    public async Task<WeatherDto?> GetCurrentAsync(string city)
    {
        if (!Cities.TryGetValue(city, out var coords))
            return null;

        var cacheKey = $"weather:{city.ToLowerInvariant()}";
        if (cache.TryGetValue(cacheKey, out WeatherDto? cached))
            return cached;

        var response = await httpClient.GetFromJsonAsync<OpenMeteoResponse>(
            $"v1/forecast?latitude={coords.Lat}&longitude={coords.Lon}&current_weather=true");

        if (response?.CurrentWeather is null)
            throw new HttpRequestException("Open-Meteo returned an empty response.");

        var dto = new WeatherDto(
            city.ToLowerInvariant(),
            response.CurrentWeather.Temperature,
            response.CurrentWeather.WindSpeed,
            DateTime.UtcNow);

        cache.Set(cacheKey, dto, TimeSpan.FromMinutes(10));
        return dto;
    }

    private sealed record OpenMeteoResponse(
        [property: JsonPropertyName("current_weather")] CurrentWeatherData? CurrentWeather);

    private sealed record CurrentWeatherData(
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("windspeed")] double WindSpeed);
}
