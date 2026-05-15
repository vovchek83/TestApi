# GeoTesterApi Benchmarks

BenchmarkDotNet performance testing suite for GeoTesterApi services.

## Overview

The benchmark suite includes performance tests for three core services:

### 1. **LosServiceBenchmarks**

Tests Line-of-Sight calculation performance with varying parameters:

- Small sample count (50 points)
- Medium sample count (500 points)
- Large sample count (2000 points)
- High frequency calculations (28 GHz)
- Low frequency calculations (400 MHz)

### 2. **PolygonServiceBenchmarks**

Tests polygon generation and spatial operations:

- Simple rectangular polygon creation
- Multi-polygon with 2 shapes
- Polygon with hole (donut shape)
- WKT parsing and multi-polygon creation

### 3. **SrtmElevationServiceBenchmarks**

Tests SRTM elevation data retrieval performance:

- Single elevation lookup
- 3×3 grid of lookups
- 10×10 grid of lookups (100 points)
- Line-based sequential lookups (100 points)
- Boundary/tile edge lookups
- Cache hit performance (same-tile lookups)

## Running Benchmarks

### Build in Release Mode

Benchmarks must run in Release configuration for accurate results:

```bash
dotnet build -c Release
```

### Run All Benchmarks

```bash
dotnet run --configuration Release -- benchmark
```

### Run Specific Benchmark Suite

```bash
# Run only LOS Service benchmarks
dotnet run --configuration Release -- benchmark LosService

# Run only Polygon Service benchmarks
dotnet run --configuration Release -- benchmark PolygonService

# Run only SRTM Elevation Service benchmarks
dotnet run --configuration Release -- benchmark SrtmElevation
```

### Using BenchmarkDotNet CLI Directly

For more control over benchmark execution:

```bash
# Run with custom configuration
dotnet run -c Release -- --job Short
dotnet run -c Release -- --warmupCount 5 --targetCount 10
dotnet run -c Release -- --memory
dotnet run -c Release -- --outliers Remove
```

## Benchmark Configuration

Current configuration (in benchmark classes):

- **Warmup**: 3 iterations
- **Target**: 5 iterations
- **Memory Diagnostics**: Enabled (tracks memory allocations)
- **Statistics**: Min, Max, Mean, Median

Modify `[SimpleJob]` attributes in individual benchmark classes to adjust.

## Output

Benchmark results are stored in:

- Terminal console output (detailed table)
- HTML report: `./BenchmarkDotNet.Artifacts/results/index.html`
- JSON: `./BenchmarkDotNet.Artifacts/BenchmarkDotNet.json`
- CSV: `./BenchmarkDotNet.Artifacts/results-*.csv`

## Interpretation Guide

### Key Metrics

- **Mean**: Average execution time (primary metric)
- **Median**: Middle value (robust to outliers)
- **Min/Max**: Range of execution times
- **Allocated**: Total memory allocated per invocation

### Performance Tips

1. **LOS Calculations**: Time scales roughly linearly with sample count
2. **Elevation Lookups**: Cache hits dramatically faster than cache misses
3. **Polygon Operations**: WKT parsing is slower than direct polygon creation
4. **Tile Boundaries**: Lookups spanning tile boundaries slightly slower

## Adding New Benchmarks

1. Create new benchmark class in `Benchmarks/` folder
2. Decorate with `[MemoryDiagnoser]` and `[SimpleJob]`
3. Mark setup method with `[GlobalSetup]`
4. Mark benchmark methods with `[Benchmark]`
5. Add type to `BenchmarkRunner.cs`

Example:

```csharp
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class MyServiceBenchmarks
{
    private MyService _service;

    [GlobalSetup]
    public void Setup()
    {
        _service = new MyService();
    }

    [Benchmark]
    public int MyBenchmark()
    {
        return _service.CalculateSomething();
    }
}
```

## Prerequisites

- .NET 8.0 SDK
- SRTM data files in `./srtm/` directory (for elevation benchmarks)
- Sufficient disk space for benchmark artifacts

## References

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Best Practices](https://benchmarkdotnet.org/articles/overview.html)
- [GitHub Repository](https://github.com/dotnet/BenchmarkDotNet)
