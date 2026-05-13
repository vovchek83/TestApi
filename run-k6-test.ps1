#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs k6 load and performance tests on the GeoTesterAPI
.DESCRIPTION
    This script provides an easy way to run k6 tests with various configurations
.PARAMETER vus
    Number of virtual users (default: 50)
.PARAMETER duration
    Test duration (default: 4m)
.PARAMETER baseUrl
    Base URL of the API (default: http://localhost:5000)
.PARAMETER reportHtml
    Generate HTML report
.PARAMETER reportJson
    Generate JSON report
.EXAMPLE
    .\run-k6-test.ps1
    .\run-k6-test.ps1 -vus 100 -duration 5m -reportHtml
#>

param(
    [int]$vus = 50,
    [string]$duration = "4m",
    [string]$baseUrl = "http://localhost:5000",
    [switch]$reportHtml,
    [switch]$reportJson
)

# Add k6 to PATH if needed
$k6Path = "C:\Program Files\k6"
if ($env:Path -notlike "*$k6Path*") {
    $env:Path += ";$k6Path"
}

# Check if k6 is installed
if (-not (Get-Command k6 -ErrorAction SilentlyContinue)) {
    Write-Host "✗ k6 not found. Install with:" -ForegroundColor Red
    Write-Host "  winget install GrafanaLabs.k6" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ k6 is ready" -ForegroundColor Green

# Build k6 command arguments
$k6Args = @(
    "run",
    "k6-load-test.js",
    "--vus", $vus,
    "--duration", $duration,
    "-e", "BASE_URL=$baseUrl"
)

# Add report options
if ($reportHtml) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportFile = "k6-report-$timestamp.html"
    $k6Args += "--out", "html=$reportFile"
    Write-Host "HTML report: $reportFile" -ForegroundColor Cyan
}

if ($reportJson) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $reportFile = "k6-results-$timestamp.json"
    $k6Args += "--out", "json=$reportFile"
    Write-Host "JSON report: $reportFile" -ForegroundColor Cyan
}

# Display configuration
Write-Host "`n=== K6 Load Test Configuration ===" -ForegroundColor Cyan
Write-Host "Virtual Users: $vus"
Write-Host "Duration: $duration"
Write-Host "Base URL: $baseUrl"
Write-Host "====================================`n" -ForegroundColor Cyan

# Run test
Write-Host "Starting k6 load test..." -ForegroundColor Green
& k6 @k6Args

# Check result
if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✓ Load test completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`n✗ Load test failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
