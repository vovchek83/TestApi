# K6 Load Testing Guide

This document provides instructions for running load and performance tests on the GeoTesterAPI using k6.

## Prerequisites

### Install k6

**Windows (using Chocolatey):**

```powershell
choco install k6
```

**Windows (Manual Download):**

- Download from: https://github.com/grafana/k6/releases
- Extract and add to PATH

**Verify Installation:**

```powershell
k6 version
```

## Running the Tests

### 1. Start Your API

First, ensure your GeoTesterAPI is running:

```powershell
cd d:\projects\TesterApi\TesterApi
dotnet run
```

The API will start on `http://localhost:5000` by default.

### 2. Run the k6 Load Test

In a new PowerShell terminal:

```powershell
cd d:\projects\TesterApi
k6 run k6-load-test.js
```

### 3. Custom Configuration

**Change virtual users and duration:**

```powershell
k6 run k6-load-test.js --vus 100 --duration 5m
```

**Change API base URL:**

```powershell
k6 run k6-load-test.js -e BASE_URL=http://localhost:5001
```

**Generate HTML Report:**

```powershell
k6 run k6-load-test.js --out html=k6-report.html
```

**Generate JSON Report:**

```powershell
k6 run k6-load-test.js --out json=k6-results.json
```

## Test Structure

The `k6-load-test.js` file tests three API groups:

### Geo API Tests

- `GET /geo/polygon` - Get polygon geometry
- `GET /geo/multipolygon` - Get multipolygon geometry
- `GET /geo/donut` - Get donut (polygon with hole)
- `GET /geo/wkt-multipolygon` - Get WKT multipolygon
- `POST /geo/union` - Union polygons

### LOS API Tests

- `POST /los/calculate` - Calculate line-of-sight profile
- `POST /los/viewshed` - Calculate viewshed polygon

### SRTM API Tests

- `GET /srtm/status` - Check SRTM tile status

## Load Test Stages

The test includes 4 stages:

1. **Ramp-up (30s):** 0 → 10 users
2. **Ramp-up (1m):** 10 → 50 users
3. **Soak (2m):** 50 users steady
4. **Ramp-down (30s):** 50 → 0 users

**Total Duration:** ~4 minutes

## Performance Thresholds

The test enforces:

- **Response time:** 95th percentile < 500ms, 99th percentile < 1000ms
- **Error rate:** < 10%

## Interpreting Results

After running the test, look for:

- `http_req_duration` - Response times
- `http_req_failed` - Failed requests
- `errors` - Custom error rate
- `geo_latency`, `los_latency`, `srtm_latency` - Latency by API group

## Quick Start Script

Run the provided `run-k6-test.ps1` script:

```powershell
.\run-k6-test.ps1 -vus 50 -duration 5m -reportHtml
```

## Troubleshooting

**k6: command not found**

- Ensure k6 is installed and in PATH
- Restart PowerShell after installation

**Connection refused**

- Ensure your API is running on the correct port
- Verify BASE_URL environment variable

**Template error**

- Ensure the API is returning valid JSON
- Check API logs for errors

## Resources

- K6 Documentation: https://k6.io/docs/
- K6 Best Practices: https://k6.io/docs/testing-guides/load-testing-best-practices/
- HTTP API Reference: https://k6.io/docs/javascript-api/k6-http/
