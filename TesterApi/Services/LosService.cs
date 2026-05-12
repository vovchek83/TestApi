using GeoTesterApi.Models;

namespace GeoTesterApi.Services;

/// <summary>
/// Computes a Radio Line-of-Sight profile between two points.
///
/// Algorithm:
///  1. Sample N points along the great-circle path.
///  2. Fetch terrain elevation (AMSL) at each sample from SRTM data.
///  3. Apply Earth-bulge correction:  h_bulge = d * (D - d) / (2 * k * Re)
///     where k = 4/3 (standard atmosphere refractivity model).
///  4. Compute first Fresnel zone radius at each point.
///  5. A point is blocked when:  terrain + bulge  >  beam - 0.6 * F1
///     (60% Fresnel zone clearance rule for radio LOS).
/// </summary>
public class LosService : ILosService
{
    private const double EarthRadiusM = 6_371_000.0;
    private const double K = 4.0 / 3.0;          // effective Earth radius factor
    private const double SpeedOfLight = 299_792_458.0;

    private readonly IElevationService _elevation;

    public LosService(IElevationService elevation) => _elevation = elevation;

    public LosResponse Calculate(LosRequest req)
    {
        int n = Math.Clamp(req.Samples, 2, 2000);

        double txTerrain = _elevation.GetElevation(req.Tx.Lat, req.Tx.Lon);
        double rxTerrain = _elevation.GetElevation(req.Rx.Lat, req.Rx.Lon);
        double txAmsl = txTerrain + req.Tx.HeightAgl;
        double rxAmsl = rxTerrain + req.Rx.HeightAgl;

        double totalM = HaversineM(req.Tx.Lat, req.Tx.Lon, req.Rx.Lat, req.Rx.Lon);
        double lambda = SpeedOfLight / (req.FrequencyMhz * 1e6); // wavelength (m)

        var profile = new List<LosProfilePoint>(n);
        LosProfilePoint? firstBlock = null;
        bool isLos = true;

        for (int i = 0; i < n; i++)
        {
            double f = (double)i / (n - 1);
            double d = f * totalM;          // distance from TX (m)
            double d2 = totalM - d;         // distance to RX (m)

            var (lat, lon) = GreatCircleIntermediate(
                req.Tx.Lat, req.Tx.Lon, req.Rx.Lat, req.Rx.Lon, f);

            double terrain = _elevation.GetElevation(lat, lon);

            // Earth-bulge correction (meters)
            double bulge = (d * d2) / (2.0 * K * EarthRadiusM);
            double effectiveTerrain = terrain + bulge;

            // Beam height: linear interpolation between TX and RX AMSL
            double beam = txAmsl + f * (rxAmsl - txAmsl);

            // First Fresnel zone radius (m) — zero at endpoints
            double fresnel = (d > 0 && d2 > 0)
                ? Math.Sqrt(lambda * d * d2 / totalM)
                : 0.0;

            // Clearance: positive = clear, negative = obstructed
            double clearance = beam - effectiveTerrain - 0.6 * fresnel;
            bool blocked = clearance < 0;

            if (blocked) isLos = false;

            var pt = new LosProfilePoint
            {
                DistanceKm = d / 1000.0,
                Lat = lat,
                Lon = lon,
                TerrainAmsl = terrain,
                EffectiveTerrainAmsl = effectiveTerrain,
                BeamAmsl = beam,
                FresnelRadius = fresnel,
                FresnelClearance = clearance,
                Blocked = blocked,
            };

            profile.Add(pt);
            firstBlock ??= blocked ? pt : null;
        }

        return new LosResponse
        {
            IsLos = isLos,
            DistanceKm = totalM / 1000.0,
            TxAmsl = txAmsl,
            RxAmsl = rxAmsl,
            Profile = profile,
            FirstObstruction = firstBlock,
        };
    }

    // ── Viewshed ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Shoots rays in every direction from TX at the given angular resolution.
    /// For each ray, walks the terrain sample-by-sample and tracks the running
    /// maximum horizon angle. A sample point is "visible" when its RX elevation
    /// angle exceeds the maximum terrain angle seen so far (i.e., no terrain peak
    /// between TX and that point blocks the line of sight).
    /// The polygon boundary in each direction is the farthest continuously-visible
    /// point, or maxRange when the entire ray is clear.
    /// Earth-bulge correction (k = 4/3) is applied to all terrain heights.
    /// </summary>
    public ViewshedResponse CalculateViewshed(ViewshedRequest req)
    {
        req.AngularResolutionDeg = Math.Clamp(req.AngularResolutionDeg, 0.1, 10.0);
        req.SamplesPerRay       = Math.Clamp(req.SamplesPerRay, 50, 1000);
        req.MaxRangeKm          = Math.Clamp(req.MaxRangeKm, 0.5, 200.0);

        double txTerrain = _elevation.GetElevation(req.Tx.Lat, req.Tx.Lon);
        double txAmsl    = txTerrain + req.Tx.HeightAgl;
        double maxRangeM = req.MaxRangeKm * 1000.0;
        double stepM     = maxRangeM / req.SamplesPerRay;

        var ring = new List<double[]>();

        for (double azimuth = 0; azimuth < 360.0; azimuth += req.AngularResolutionDeg)
        {
            double maxHorizonAngle = double.NegativeInfinity;
            double visLat = req.Tx.Lat;
            double visLon = req.Tx.Lon;

            for (int s = 1; s <= req.SamplesPerRay; s++)
            {
                double d = s * stepM;
                var (lat, lon) = Destination(req.Tx.Lat, req.Tx.Lon, azimuth, d);
                double terrain  = _elevation.GetElevation(lat, lon);
                double curve    = (d * d) / (2.0 * K * EarthRadiusM);

                // RX antenna at this position, corrected for Earth bulge
                double rxAmsl      = terrain + req.RxHeightAgl + curve;
                double rxAngle     = Math.Atan2(rxAmsl - txAmsl, d);

                // Point is visible if its angle exceeds every prior terrain peak
                if (rxAngle > maxHorizonAngle)
                    (visLat, visLon) = (lat, lon);

                // Update horizon with terrain at this point
                double horizonAngle = Math.Atan2(terrain + curve - txAmsl, d);
                if (horizonAngle > maxHorizonAngle)
                    maxHorizonAngle = horizonAngle;
            }

            ring.Add([visLon, visLat]); // GeoJSON: [lon, lat]
        }

        // Close the ring
        ring.Add(ring[0]);

        // Approximate coverage area (shoelace on a sphere is complex; use planar approx)
        double areaKm2 = ShoelaceAreaKm2(ring);

        return new ViewshedResponse
        {
            PolygonCoords  = ring,
            RayCount       = ring.Count - 1,
            TxAmsl         = txAmsl,
            CoverageAreaKm2 = Math.Abs(areaKm2),
        };
    }

    /// <summary>
    /// Returns the destination point given a start, bearing (degrees CW from N), and distance (m).
    /// </summary>
    private static (double lat, double lon) Destination(
        double lat1, double lon1, double bearingDeg, double distanceM)
    {
        double d  = distanceM / EarthRadiusM;
        double br = Rad(bearingDeg);
        double φ1 = Rad(lat1);
        double λ1 = Rad(lon1);

        double φ2 = Math.Asin(
            Math.Sin(φ1) * Math.Cos(d) +
            Math.Cos(φ1) * Math.Sin(d) * Math.Cos(br));

        double λ2 = λ1 + Math.Atan2(
            Math.Sin(br) * Math.Sin(d) * Math.Cos(φ1),
            Math.Cos(d) - Math.Sin(φ1) * Math.Sin(φ2));

        return (Deg(φ2), Deg(λ2));
    }

    private static double ShoelaceAreaKm2(List<double[]> ring)
    {
        // Planar shoelace in degree-units, convert to km²
        double area = 0;
        for (int i = 0; i < ring.Count - 1; i++)
        {
            double x0 = ring[i][0], y0 = ring[i][1];
            double x1 = ring[i + 1][0], y1 = ring[i + 1][1];
            area += x0 * y1 - x1 * y0;
        }
        // 1° lat ≈ 111 km, 1° lon ≈ 111*cos(lat) km
        double midLat = ring.Average(c => c[1]);
        double kmPerDegLat = 111.0;
        double kmPerDegLon = 111.0 * Math.Cos(Rad(midLat));
        return Math.Abs(area / 2.0) * kmPerDegLat * kmPerDegLon;
    }

    // ── Geodesic helpers ────────────────────────────────────────────────────

    private static double HaversineM(double lat1, double lon1, double lat2, double lon2)
    {
        double dLat = Rad(lat2 - lat1);
        double dLon = Rad(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * EarthRadiusM * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static (double lat, double lon) GreatCircleIntermediate(
        double lat1, double lon1, double lat2, double lon2, double f)
    {
        double δ = HaversineM(lat1, lon1, lat2, lon2) / EarthRadiusM;
        if (δ < 1e-10) return (lat1, lon1);

        double sinδ = Math.Sin(δ);
        double A = Math.Sin((1 - f) * δ) / sinδ;
        double B = Math.Sin(f * δ) / sinδ;

        double φ1 = Rad(lat1), λ1 = Rad(lon1);
        double φ2 = Rad(lat2), λ2 = Rad(lon2);

        double x = A * Math.Cos(φ1) * Math.Cos(λ1) + B * Math.Cos(φ2) * Math.Cos(λ2);
        double y = A * Math.Cos(φ1) * Math.Sin(λ1) + B * Math.Cos(φ2) * Math.Sin(λ2);
        double z = A * Math.Sin(φ1) + B * Math.Sin(φ2);

        double latOut = Deg(Math.Atan2(z, Math.Sqrt(x * x + y * y)));
        double lonOut = Deg(Math.Atan2(y, x));
        return (latOut, lonOut);
    }

    private static double Rad(double deg) => deg * Math.PI / 180.0;
    private static double Deg(double rad) => rad * 180.0 / Math.PI;
}
