using BenchmarkDotNet.Attributes;
using GeoTesterApi.Services;
using GeoToolkit;
using GeoToolkit.Interfaces;

namespace GeoTesterApi.Benchmarks;

/// <summary>
/// Benchmarks for polygon generation and manipulation.
/// Tests performance of various polygon creation and union operations.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class PolygonServiceBenchmarks
{
    private IPolygonService _polygonService = null!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ServiceCollection();
        builder.AddGeoToolkit();
        var services = builder.BuildServiceProvider();

        var operationsService = services.GetRequiredService<IGeometryOperationsService>();
        var geoUnionService = services.GetRequiredService<IGeoUnionService>();

        _polygonService = new PolygonService(operationsService, geoUnionService);
    }

    [Benchmark(Description = "Create simple rectangular polygon")]
    public void CreatePolygon()
    {
        _polygonService.CreatePolygon();
    }

    [Benchmark(Description = "Create multi-polygon with 2 rectangles")]
    public void CreateMultiPolygon()
    {
        _polygonService.CreateMultiPolygon();
    }

    [Benchmark(Description = "Create donut polygon with hole")]
    public void CreateDonutPolygon()
    {
        _polygonService.CreateDonutPolygon();
    }

    [Benchmark(Description = "Create and parse multi-polygon from WKT")]
    public void CreateMultiPolygonFromWkt()
    {
        _polygonService.CreateMultiPolygonFromWkt();
    }
}
