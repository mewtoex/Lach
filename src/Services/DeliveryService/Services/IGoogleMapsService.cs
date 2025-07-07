namespace DeliveryService.Services;

public interface IGoogleMapsService
{
    Task<GeocodingResult> GeocodeAddressAsync(string address);
    Task<double> CalculateDistanceAsync(double originLat, double originLng, double destLat, double destLng);
    Task<DistanceMatrixResult> GetDistanceMatrixAsync(string origin, string destination);
}

public class GeocodingResult
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string FormattedAddress { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}

public class DistanceMatrixResult
{
    public double DistanceInKm { get; set; }
    public int DurationInMinutes { get; set; }
    public bool IsValid { get; set; }
} 