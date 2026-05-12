namespace GeoTesterApi.Services;

public interface IElevationService
{
    /// <summary>Returns terrain elevation in meters AMSL. Returns 0 if no data file found.</summary>
    double GetElevation(double lat, double lon);
}
