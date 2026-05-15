#Requires -Version 5.0
<#
.SYNOPSIS
    Run BenchmarkDotNet in Docker container

.DESCRIPTION
    Builds and runs BenchmarkDotNet benchmarks in a Docker container for GeoTesterApi

.PARAMETER Filter
    Optional filter to run specific benchmarks (e.g., "LosService", "Polygon")

.EXAMPLE
    .\run-benchmark-docker.ps1
    .\run-benchmark-docker.ps1 -Filter "LosService"
#>

param(
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"

$dockerImage = "geotester-bench"
$resultsDir = (Join-Path (Get-Location) "benchmark-results")
$srtmDir = (Join-Path (Get-Location) "srtm")

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  GeoTesterApi Benchmark Runner (Docker)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Create results directory
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir -Force | Out-Null
}
Write-Host "✓ Results will be saved to: $resultsDir" -ForegroundColor Green
Write-Host ""

# Build Docker image
Write-Host "📦 Building Docker image: $dockerImage" -ForegroundColor Yellow
docker build -f "TesterApi/Dockerfile.benchmark" -t $dockerImage .
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build Docker image" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Docker image built" -ForegroundColor Green
Write-Host ""

# Run benchmarks
Write-Host "🚀 Running benchmarks..." -ForegroundColor Yellow
if ([string]::IsNullOrWhiteSpace($Filter)) {
    Write-Host "   Running ALL benchmarks (LOS, Polygon, SRTM Elevation)..." -ForegroundColor Cyan
    
    $volumeArgs = @(
        "-v", "$($resultsDir):/app/BenchmarkDotNet.Artifacts"
    )
    
    # Add SRTM mount if directory exists
    if (Test-Path $srtmDir) {
        $volumeArgs += @("-v", "$($srtmDir):/app/srtm:ro")
    }
    
    docker run --rm @volumeArgs $dockerImage
} else {
    Write-Host "   Running benchmarks matching: $Filter" -ForegroundColor Cyan
    
    $volumeArgs = @(
        "-v", "$($resultsDir):/app/BenchmarkDotNet.Artifacts"
    )
    
    # Add SRTM mount if directory exists
    if (Test-Path $srtmDir) {
        $volumeArgs += @("-v", "$($srtmDir):/app/srtm:ro")
    }
    
    docker run --rm @volumeArgs $dockerImage $Filter
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Benchmark execution failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✓ Benchmarks completed!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📊 Results location: $resultsDir" -ForegroundColor Cyan
Write-Host "   - HTML report:  $resultsDir\results\index.html" -ForegroundColor Gray
Write-Host "   - JSON data:    $resultsDir\BenchmarkDotNet.json" -ForegroundColor Gray
Write-Host "   - CSV data:     $resultsDir\results-*.csv" -ForegroundColor Gray
Write-Host ""
