namespace ProductsApi.Dtos;

public record WeatherDto(string City, double Temperature, double WindSpeed, DateTime FetchedAt);
