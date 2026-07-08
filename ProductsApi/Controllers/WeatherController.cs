using Microsoft.AspNetCore.Mvc;
using ProductsApi.Dtos;
using ProductsApi.Services;

namespace ProductsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController(IWeatherService service) : ControllerBase
{
    [HttpGet("{city}")]
    public async Task<ActionResult<WeatherDto>> Get(string city)
    {
        WeatherDto? weather;
        try
        {
            weather = await service.GetCurrentAsync(city);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return Problem(
                title: "Weather service unavailable",
                detail: "The external weather provider did not respond.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (weather is null)
        {
            return Problem(
                title: "City not supported",
                detail: $"Supported cities: {string.Join(", ", service.SupportedCities)}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(weather);
    }
}
