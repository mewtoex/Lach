using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace DeliveryService.Services;

public class GoogleMapsService : IGoogleMapsService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleMapsService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _apiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty;
    }

    public async Task<GeocodingResult> GeocodeAddressAsync(string address)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback to mock data for development
            return new GeocodingResult
            {
                Latitude = -23.5505,
                Longitude = -46.6333,
                FormattedAddress = address,
                IsValid = true
            };
        }

        try
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<GoogleGeocodingResponse>(response);

            if (result?.Results?.Any() == true)
            {
                var location = result.Results[0].Geometry.Location;
                return new GeocodingResult
                {
                    Latitude = location.Lat,
                    Longitude = location.Lng,
                    FormattedAddress = result.Results[0].FormattedAddress,
                    IsValid = true
                };
            }

            return new GeocodingResult { IsValid = false };
        }
        catch
        {
            return new GeocodingResult { IsValid = false };
        }
    }

    public async Task<double> CalculateDistanceAsync(double originLat, double originLng, double destLat, double destLng)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback to simple calculation for development
            return CalculateHaversineDistance(originLat, originLng, destLat, destLng);
        }

        try
        {
            var origin = $"{originLat},{originLng}";
            var destination = $"{destLat},{destLng}";
            var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&key={_apiKey}";
            
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<GoogleDistanceMatrixResponse>(response);

            if (result?.Rows?.Any() == true && result.Rows[0].Elements?.Any() == true)
            {
                var element = result.Rows[0].Elements[0];
                if (element.Status == "OK" && element.Distance?.Value > 0)
                {
                    return element.Distance.Value / 1000.0; // Convert meters to kilometers
                }
            }

            // Fallback to Haversine calculation
            return CalculateHaversineDistance(originLat, originLng, destLat, destLng);
        }
        catch
        {
            // Fallback to Haversine calculation
            return CalculateHaversineDistance(originLat, originLng, destLat, destLng);
        }
    }

    public async Task<DistanceMatrixResult> GetDistanceMatrixAsync(string origin, string destination)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback to mock data for development
            return new DistanceMatrixResult
            {
                DistanceInKm = 5.0,
                DurationInMinutes = 15,
                IsValid = true
            };
        }

        try
        {
            var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={Uri.EscapeDataString(origin)}&destinations={Uri.EscapeDataString(destination)}&key={_apiKey}";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<GoogleDistanceMatrixResponse>(response);

            if (result?.Rows?.Any() == true && result.Rows[0].Elements?.Any() == true)
            {
                var element = result.Rows[0].Elements[0];
                if (element.Status == "OK")
                {
                    return new DistanceMatrixResult
                    {
                        DistanceInKm = element.Distance?.Value / 1000.0 ?? 0,
                        DurationInMinutes = element.Duration?.Value / 60 ?? 0,
                        IsValid = true
                    };
                }
            }

            return new DistanceMatrixResult { IsValid = false };
        }
        catch
        {
            return new DistanceMatrixResult { IsValid = false };
        }
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}

// Google Maps API Response Models
public class GoogleGeocodingResponse
{
    public List<GeocodingResult> Results { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class GeocodingResult
{
    public string FormattedAddress { get; set; } = string.Empty;
    public Geometry Geometry { get; set; } = new();
}

public class Geometry
{
    public Location Location { get; set; } = new();
}

public class Location
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}

public class GoogleDistanceMatrixResponse
{
    public List<Row> Rows { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class Row
{
    public List<Element> Elements { get; set; } = new();
}

public class Element
{
    public Distance Distance { get; set; } = new();
    public Duration Duration { get; set; } = new();
    public string Status { get; set; } = string.Empty;
}

public class Distance
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class Duration
{
    public int Value { get; set; }
    public string Text { get; set; } = string.Empty;
} 