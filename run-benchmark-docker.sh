#!/bin/bash

# run-benchmark-docker.sh - Run BenchmarkDotNet in Docker container
# Usage: ./run-benchmark-docker.sh [benchmark-filter]
# Example: ./run-benchmark-docker.sh LosService

set -e

DOCKER_IMAGE="geotester-bench"
BENCHMARK_FILTER=${1:-""}
RESULTS_DIR="$(pwd)/benchmark-results"

echo "═══════════════════════════════════════════════════════════"
echo "  GeoTesterApi Benchmark Runner (Docker)"
echo "═══════════════════════════════════════════════════════════"
echo ""

# Create results directory
mkdir -p "$RESULTS_DIR"
echo "✓ Results will be saved to: $RESULTS_DIR"
echo ""

# Build Docker image
echo "📦 Building Docker image: $DOCKER_IMAGE"
docker build -f Dockerfile.benchmark -t "$DOCKER_IMAGE" .
echo "✓ Docker image built"
echo ""

# Run benchmarks
echo "🚀 Running benchmarks..."
if [ -z "$BENCHMARK_FILTER" ]; then
    echo "   Running ALL benchmarks (LOS, Polygon, SRTM Elevation)..."
    docker run --rm \
        -v "$RESULTS_DIR":/app/BenchmarkDotNet.Artifacts \
        -v "$(pwd)/srtm":/app/srtm:ro 2>/dev/null || true \
        "$DOCKER_IMAGE"
else
    echo "   Running benchmarks matching: $BENCHMARK_FILTER"
    docker run --rm \
        -v "$RESULTS_DIR":/app/BenchmarkDotNet.Artifacts \
        -v "$(pwd)/srtm":/app/srtm:ro 2>/dev/null || true \
        "$DOCKER_IMAGE" "$BENCHMARK_FILTER"
fi

echo ""
echo "═══════════════════════════════════════════════════════════"
echo "  ✓ Benchmarks completed!"
echo "═══════════════════════════════════════════════════════════"
echo ""
echo "📊 Results location: $RESULTS_DIR"
echo "   - HTML report:  $RESULTS_DIR/results/index.html"
echo "   - JSON data:    $RESULTS_DIR/BenchmarkDotNet.json"
echo "   - CSV data:     $RESULTS_DIR/results-*.csv"
echo ""
