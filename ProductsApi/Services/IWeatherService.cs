using ProductsApi.Dtos;

namespace ProductsApi.Services;

public interface IWeatherService
{
    IEnumerable<string> SupportedCities { get; }
    Task<WeatherDto?> GetCurrentAsync(string city);
}
