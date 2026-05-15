using BenchmarkDotNet.Attributes;
using GeoTesterApi.Models;
using GeoTesterApi.Services;

namespace GeoTesterApi.Benchmarks;

/// <summary>
/// Benchmarks for LOS (Line-of-Sight) calculation service.
/// Tests performance of the LOS profile calculation with different sample rates.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class LosServiceBenchmarks
{
    private IElevationService _elevationService = null!;
    private ILosService _losService = null!;

    // Test parameters
    private static readonly LosRequest SmallSamples = new()
    {
        Tx = new() { Lat = 50.45, Lon = 30.52, HeightAgl = 50 },
        Rx = new() { Lat = 50.48, Lon = 30.55, HeightAgl = 30 },
        FrequencyMhz = 2400,
        Samples = 50
    };

    private static readonly LosRequest MediumSamples = new()
    {
        Tx = new() { Lat = 50.45, Lon = 30.52, HeightAgl = 50 },
        Rx = new() { Lat = 50.48, Lon = 30.55, HeightAgl = 30 },
        FrequencyMhz = 2400,
        Samples = 500
    };

    private static readonly LosRequest LargeSamples = new()
    {
        Tx = new() { Lat = 50.45, Lon = 30.52, HeightAgl = 50 },
        Rx = new() { Lat = 50.48, Lon = 30.55, HeightAgl = 30 },
        FrequencyMhz = 2400,
        Samples = 2000
    };

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
        _losService = new LosService(_elevationService);
    }

    [Benchmark(Description = "LOS calculation with 50 samples")]
    public LosResponse Calculate_SmallSamples()
    {
        return _losService.Calculate(SmallSamples);
    }

    [Benchmark(Description = "LOS calculation with 500 samples")]
    public LosResponse Calculate_MediumSamples()
    {
        return _losService.Calculate(MediumSamples);
    }

    [Benchmark(Description = "LOS calculation with 2000 samples")]
    public LosResponse Calculate_LargeSamples()
    {
        return _losService.Calculate(LargeSamples);
    }

    [Benchmark(Description = "LOS calculation with high frequency (28 GHz)")]
    public LosResponse Calculate_HighFrequency()
    {
        var highFreqRequest = new LosRequest
        {
            Tx = SmallSamples.Tx,
            Rx = SmallSamples.Rx,
            FrequencyMhz = 28000,
            Samples = SmallSamples.Samples
        };
        return _losService.Calculate(highFreqRequest);
    }

    [Benchmark(Description = "LOS calculation with low frequency (400 MHz)")]
    public LosResponse Calculate_LowFrequency()
    {
        var lowFreqRequest = new LosRequest
        {
            Tx = SmallSamples.Tx,
            Rx = SmallSamples.Rx,
            FrequencyMhz = 400,
            Samples = SmallSamples.Samples
        };
        return _losService.Calculate(lowFreqRequest);
    }
}
