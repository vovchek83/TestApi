# BenchmarkDotNet Setup & Usage Guide

## What Was Added

BenchmarkDotNet integration has been added to GeoTesterApi with comprehensive performance test suites for all major services.

### New Files Created

```
Benchmarks/
├── README.md                          # Detailed benchmark documentation
├── LosServiceBenchmarks.cs           # LOS calculation benchmarks
├── PolygonServiceBenchmarks.cs       # Polygon operation benchmarks
└── SrtmElevationServiceBenchmarks.cs # Elevation lookup benchmarks

BenchmarkRunner.cs                     # Benchmark orchestration utility
```

### Project File Updated

- **GeoTesterApi.csproj**: Added `BenchmarkDotNet` NuGet package (v0.13.2)
- **Program.cs**: Added "benchmark" command-line mode to trigger benchmarks

## Quick Start

### 1. Build in Release Mode

```powershell
cd d:\projects\TesterApi\TesterApi
dotnet build -c Release
```

### 2. Run All Benchmarks

```powershell
dotnet run -c Release -- benchmark
```

### 3. Run Specific Benchmark Suite

```powershell
# LOS Service only
dotnet run -c Release -- benchmark LosService

# Polygon Service only
dotnet run -c Release -- benchmark PolygonService

# SRTM Elevation only
dotnet run -c Release -- benchmark SrtmElevation
```

## Benchmark Suites Included

### LosServiceBenchmarks

- **50 samples**: Small profile (quick calculation)
- **500 samples**: Medium profile
- **2000 samples**: Maximum recommended samples
- **High frequency (28 GHz)**: Millimeter-wave LOS
- **Low frequency (400 MHz)**: VHF LOS

### PolygonServiceBenchmarks

- Simple rectangle creation
- Multi-polygon (2 shapes)
- Polygon with hole
- WKT parsing

### SrtmElevationServiceBenchmarks

- Single point lookup
- 3×3 grid (9 points)
- 10×10 grid (100 points)
- Sequential line (100 points)
- Boundary lookups (tile edges)
- Cache hit performance

## Output & Results

Benchmark results are saved to:

- **Console**: Live output with detailed metrics
- **HTML**: `./BenchmarkDotNet.Artifacts/results/index.html`
- **JSON**: `./BenchmarkDotNet.Artifacts/BenchmarkDotNet.json`
- **CSV**: `./BenchmarkDotNet.Artifacts/results-*.csv`

## Key Metrics Explained

| Metric        | Description                             |
| ------------- | --------------------------------------- |
| **Mean**      | Average execution time (primary metric) |
| **Median**    | Middle value (robust to outliers)       |
| **Min/Max**   | Range of execution times                |
| **Allocated** | Memory allocated per invocation (bytes) |

## Performance Insights

From these benchmarks you can analyze:

1. **Scalability**: How LOS time increases with sample count
2. **Caching**: SRTM cache hit vs. miss performance
3. **Operations**: Polygon operation costs
4. **Frequency Dependence**: Radio calculations at different frequencies
5. **Memory**: Allocation patterns and GC pressure

## Advanced Usage

### Custom Benchmark Configuration

Edit benchmark class attributes to customize:

```csharp
[SimpleJob(warmupCount: 5, targetCount: 10)]  // More iterations
[MemoryDiagnoser]  // Enable memory profiling
[ShortRunJob]       // Quick estimates
```

### Filter by Benchmark Name

```powershell
# Benchmarks containing "Cache"
dotnet run -c Release -- benchmark Cache
```

### Running with Web Server

Benchmarks prevent the web server from starting. To run both:

```powershell
# Terminal 1: Start web server (normal mode)
dotnet run

# Terminal 2: Run benchmarks in Release mode (separate process)
dotnet run -c Release -- benchmark
```

## Troubleshooting

### "Benchmark completed, but no results"

- Ensure running in **Release mode** (`-c Release`)
- Check that SRTM data files exist in `./srtm/` (for elevation benchmarks)

### Out of Memory

- Reduce sample counts in benchmark configuration
- Run one benchmark suite at a time

### SRTM Data Not Found

- Elevation benchmarks will use default elevation (0 meters) if files missing
- Download SRTM tiles from: https://dwtkns.com/srtm30m/
- Place in `./srtm/` directory

## Running Benchmarks in Docker

⚠️ **Docker Status**: Docker builds require NuGet authentication for the GeoToolkit private package (v1.0.2). Use native benchmarking for development.

### Option 1: Native Benchmarks (Recommended for Development)

Run benchmarks directly on your machine:

```powershell
# Build in Release  mode
dotnet build GeoTesterApi.csproj -c Release

# Run all benchmarks
dotnet run -c Release -- benchmark

# Run specific benchmarks
dotnet run -c Release -- benchmark LosService
dotnet run -c Release -- benchmark PolygonService
dotnet run -c Release -- benchmark SrtmElevation
```

### Option 2: Docker for CI/CD (with NuGet Configuration)

To use Docker in CI/CD pipelines, add NuGet authentication to `TesterApi/Dockerfile.benchmark`:

```dockerfile
RUN dotnet nuget add source "https://your-nuget-feed" -n "PrivateFeed" \
    -u "$NUGET_USER" -p "$NUGET_PASSWORD" --store-password-in-clear-text
```

Build with legacy Docker builder (Windows):

```powershell
$env:DOCKER_BUILDKIT=0
docker build -f "TesterApi\Dockerfile.benchmark" -t geotester-bench .
docker run --rm -v $PWD/benchmark-results:/app/BenchmarkDotNet.Artifacts geotester-bench
```

Or on Linux/Mac:

```bash
docker build -f TesterApi/Dockerfile.benchmark -t geotester-bench .
docker run --rm \
  -v $(pwd)/benchmark-results:/app/BenchmarkDotNet.Artifacts \
  -v $(pwd)/srtm:/app/srtm:ro \
  geotester-bench
```

### Docker Performance Considerations

| Factor          | Impact                           |
| --------------- | -------------------------------- |
| **CPU Limits**  | Can skew results                 |
| **Memory**      | Affects GC/allocation benchmarks |
| **I/O**         | Slower than native               |
| **Consistency** | Best on quiet, dedicated systems |

**Best Practice**: Use native benchmarking for accurate results. Reserve Docker for headless environments.

## Next Steps

1. Run benchmarks: `dotnet run -c Release -- benchmark`
2. Review HTML report in `./BenchmarkDotNet.Artifacts/results/index.html`
3. Add benchmarks as features are developed
4. Track metrics over time for performance trending
5. Identify optimization opportunities from results

## References

- [BenchmarkDotNet Official Docs](https://benchmarkdotnet.org/)
- [Best Practices](https://benchmarkdotnet.org/articles/overview.html)
- [Attributes Guide](https://benchmarkdotnet.org/articles/configs/attributes.html)
- [Docker Best Practices for .NET](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/container-docker-introduction/docker-containers-images-registries)

---

**Version**: 1.2  
**Updated**: 2026-05-15  
**BenchmarkDotNet**: 0.13.2  
**Docker Support**: Partial (requires NuGet authentication for private feeds)
