using BenchmarkDotNet.Running;
using GeoTesterApi.Benchmarks;

namespace GeoTesterApi;

/// <summary>
/// Utility class for running BenchmarkDotNet benchmarks.
/// Run benchmarks using: dotnet run --configuration Release -- benchmark
/// </summary>
public static class BenchmarkRunner
{
    public static void RunBenchmarks(string? filter = null)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           GeoTesterApi Performance Benchmarks               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        var allTypes = new[]
        {
            typeof(LosServiceBenchmarks),
            typeof(PolygonServiceBenchmarks),
            typeof(SrtmElevationServiceBenchmarks)
        };

        Type[] benchmarkTypes;

        if (filter == null)
        {
            Console.WriteLine("Running ALL benchmarks (LOS, Polygon, SRTM Elevation)...");
            benchmarkTypes = allTypes;
        }
        else
        {
            Console.WriteLine($"Running benchmarks matching: {filter}");
            
            benchmarkTypes = allTypes
                .Where(t => t.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (benchmarkTypes.Length == 0)
            {
                Console.WriteLine($"❌ No benchmarks found matching: {filter}");
                Console.WriteLine($"\nAvailable:");
                Console.WriteLine($"  - LosServiceBenchmarks");
                Console.WriteLine($"  - PolygonServiceBenchmarks");
                Console.WriteLine($"  - SrtmElevationServiceBenchmarks");
                return;
            }
        }

        Console.WriteLine();

        try
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run(benchmarkTypes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error running benchmarks: {ex.Message}");
            Console.WriteLine($"\n💡 Make sure to run in Release mode:");
            Console.WriteLine($"   dotnet run -c Release -- benchmark");
        }

        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Benchmark run completed!                      ║");
        Console.WriteLine("║    Results saved to: ./BenchmarkDotNet.Artifacts/           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
    }
}
