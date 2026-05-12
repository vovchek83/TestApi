namespace GeoTesterApi.Models;

public class LosPointDto
{
    public double Lat { get; set; }
    public double Lon { get; set; }
    /// <summary>Height above ground level (meters)</summary>
    public double HeightAgl { get; set; } = 10.0;
}

public class LosRequest
{
    public LosPointDto Tx { get; set; } = new();
    public LosPointDto Rx { get; set; } = new();
    /// <summary>Radio frequency in MHz (affects Fresnel zone radius)</summary>
    public double FrequencyMhz { get; set; } = 900.0;
    /// <summary>Number of profile sample points (2–2000)</summary>
    public int Samples { get; set; } = 300;
}

public class LosProfilePoint
{
    public double DistanceKm { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
    /// <summary>Raw terrain elevation above mean sea level</summary>
    public double TerrainAmsl { get; set; }
    /// <summary>Terrain + Earth curvature correction</summary>
    public double EffectiveTerrainAmsl { get; set; }
    /// <summary>Radio beam height at this point (linear TX→RX)</summary>
    public double BeamAmsl { get; set; }
    /// <summary>First Fresnel zone radius (meters)</summary>
    public double FresnelRadius { get; set; }
    /// <summary>Positive = clear, negative = obstructed (meters)</summary>
    public double FresnelClearance { get; set; }
    public bool Blocked { get; set; }
}

public class LosResponse
{
    public bool IsLos { get; set; }
    public double DistanceKm { get; set; }
    public double TxAmsl { get; set; }
    public double RxAmsl { get; set; }
    public List<LosProfilePoint> Profile { get; set; } = [];
    public LosProfilePoint? FirstObstruction { get; set; }
}

// ── Viewshed ────────────────────────────────────────────────────────────────

public class ViewshedRequest
{
    public LosPointDto Tx { get; set; } = new();
    /// <summary>Height of the target receiver above ground (meters)</summary>
    public double RxHeightAgl { get; set; } = 2.0;
    /// <summary>Maximum radius to search (km)</summary>
    public double MaxRangeKm { get; set; } = 20.0;
    /// <summary>Angular step between rays (degrees). Smaller = more detail, slower.</summary>
    public double AngularResolutionDeg { get; set; } = 1.0;
    /// <summary>Terrain samples per ray</summary>
    public int SamplesPerRay { get; set; } = 300;
}

public class ViewshedResponse
{
    /// <summary>
    /// Polygon ring as [lon, lat] pairs (GeoJSON coordinate order, ring is closed).
    /// Each pair is the farthest continuously-visible point in that ray direction.
    /// </summary>
    public List<double[]> PolygonCoords { get; set; } = [];
    public int RayCount { get; set; }
    public double TxAmsl { get; set; }
    public double CoverageAreaKm2 { get; set; }
}
