using BenchmarkDotNet.Attributes;
using GeoTesterApi.Services;

namespace GeoTesterApi.Benchmarks;

/// <summary>
/// Benchmarks for SRTM elevation data retrievals.
/// Tests performance of bilinear interpolation with various access patterns.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class SrtmElevationServiceBenchmarks
{
    private IElevationService _elevationService = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Srtm:DataPath", Path.Combine(AppContext.BaseDirectory, "srtm") }
            })
            .Build();

        _elevationService = new SrtmElevationService(config);
    }

    [Benchmark(Description = "Single elevation lookup (Kyiv area)")]
    public double GetElevation_Single()
    {
        return _elevationService.GetElevation(50.45, 30.52);
    }

    [Benchmark(Description = "Grid of 9 elevation lookups (3x3)")]
    public double GetElevation_Grid3x3()
    {
        double sum = 0;
        for (double lat = 50.44; lat <= 50.46; lat += 0.01)
        {
            for (double lon = 30.51; lon <= 30.53; lon += 0.01)
            {
                sum += _elevationService.GetElevation(lat, lon);
            }
        }
        return sum;
    }

    [Benchmark(Description = "Dense grid of 100 elevation lookups (10x10)")]
    public double GetElevation_Grid10x10()
    {
        double sum = 0;
        for (double lat = 50.40; lat <= 50.50; lat += 0.01)
        {
            for (double lon = 30.45; lon <= 30.55; lon += 0.01)
            {
                sum += _elevationService.GetElevation(lat, lon);
            }
        }
        return sum;
    }

    [Benchmark(Description = "Sequential lookups along a line (100 points)")]
    public double GetElevation_Line100()
    {
        double sum = 0;
        for (int i = 0; i < 100; i++)
        {
            double t = i / 100.0;
            double lat = 50.45 + t * 0.05;
            double lon = 30.52 + t * 0.05;
            sum += _elevationService.GetElevation(lat, lon);
        }
        return sum;
    }

    [Benchmark(Description = "Boundary test: corner and edge lookups")]
    public double GetElevation_Boundary()
    {
        double sum = 0;
        // Test tile boundaries (lookups that span tile boundaries)
        sum += _elevationService.GetElevation(50.0, 30.0);
        sum += _elevationService.GetElevation(50.0, 30.99);
        sum += _elevationService.GetElevation(50.99, 30.0);
        sum += _elevationService.GetElevation(50.99, 30.99);
        sum += _elevationService.GetElevation(50.5, 30.5);
        return sum;
    }

    [Benchmark(Description = "Same tile cache hit (multiple lookups, same tile)")]
    public double GetElevation_CacheHits()
    {
        double sum = 0;
        // All lookups in the same 1°×1° tile
        for (int i = 0; i < 50; i++)
        {
            sum += _elevationService.GetElevation(50.45 + (i * 0.001), 30.52 + (i * 0.001));
        }
        return sum;
    }
}
