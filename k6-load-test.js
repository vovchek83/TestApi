import http from "k6/http";
import { check, sleep, group } from "k6";
import { Rate, Trend, Counter } from "k6/metrics";

// Custom metrics
const errorRate = new Rate("errors");
const geoLatency = new Trend("geo_latency");
const losLatency = new Trend("los_latency");
const srtmLatency = new Trend("srtm_latency");
const successCount = new Counter("success");
const failureCount = new Counter("failures");

const BASE_URL = __ENV.BASE_URL || "http://localhost:5000";

// Test configuration
export const options = {
  stages: [
    { duration: "30s", target: 10 }, // Ramp up to 10 users
    { duration: "1m", target: 50 }, // Ramp up to 50 users
    { duration: "2m", target: 50 }, // Stay at 50 users
    { duration: "30s", target: 0 }, // Ramp down to 0 users
  ],
  thresholds: {
    http_req_duration: ["p(95)<500", "p(99)<1000"],
    http_req_failed: ["rate<0.1"],
    errors: ["rate<0.1"],
  },
};

export default function () {
  group("Geo API", () => {
    // Test GetPolygon endpoint
    let res = http.get(`${BASE_URL}/geo/polygon`);
    let success = check(res, {
      "GetPolygon status 200": (r) => r.status === 200,
      "GetPolygon has features": (r) => r.body.includes("features"),
      "GetPolygon response time < 500ms": (r) => r.timings.duration < 500,
    });
    geoLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);

    // Test GetMultiPolygon endpoint
    res = http.get(`${BASE_URL}/geo/multipolygon`);
    success = check(res, {
      "GetMultiPolygon status 200": (r) => r.status === 200,
      "GetMultiPolygon response time < 500ms": (r) => r.timings.duration < 500,
    });
    geoLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);

    // Test GetDonutPolygon endpoint
    res = http.get(`${BASE_URL}/geo/donut`);
    success = check(res, {
      "GetDonutPolygon status 200": (r) => r.status === 200,
      "GetDonutPolygon response time < 500ms": (r) => r.timings.duration < 500,
    });
    geoLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);

    // Test GetWktMultiPolygon endpoint
    res = http.get(`${BASE_URL}/geo/wkt-multipolygon`);
    success = check(res, {
      "GetWktMultiPolygon status 200": (r) => r.status === 200,
      "GetWktMultiPolygon response time < 500ms": (r) =>
        r.timings.duration < 500,
    });
    geoLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);

    // Test UnionPolygons endpoint
    const unionPayload = {
      type: "FeatureCollection",
      features: [
        {
          type: "Feature",
          geometry: {
            type: "Polygon",
            coordinates: [
              [
                [28.0, 48.0],
                [28.1, 48.0],
                [28.1, 48.1],
                [28.0, 48.1],
                [28.0, 48.0],
              ],
            ],
          },
        },
      ],
    };

    res = http.post(`${BASE_URL}/geo/union`, JSON.stringify(unionPayload), {
      headers: { "Content-Type": "application/json" },
    });
    success = check(res, {
      "UnionPolygons status 200": (r) => r.status === 200,
      "UnionPolygons response time < 500ms": (r) => r.timings.duration < 500,
    });
    geoLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);
  });

  group("LOS API", () => {
    // Test CalculateLos endpoint
    const losRequest = {
      txLat: 48.5,
      txLon: 29.5,
      txHeightAgl: 10.0,
      rxLat: 48.55,
      rxLon: 29.55,
      rxHeightAgl: 2.0,
      frequencyMhz: 2450.0,
      maxRangeKm: 5.0,
    };

    let res = http.post(
      `${BASE_URL}/los/calculate`,
      JSON.stringify(losRequest),
      { headers: { "Content-Type": "application/json" } },
    );
    let success = check(res, {
      "CalculateLos status 200": (r) => r.status === 200,
      "CalculateLos has response": (r) => r.body.length > 0,
      "CalculateLos response time < 1000ms": (r) => r.timings.duration < 1000,
    });
    losLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);

    // Test CalculateViewshed endpoint
    const viewshedRequest = {
      txLat: 48.5,
      txLon: 29.5,
      txHeightAgl: 10.0,
      frequencyMhz: 2450.0,
      maxRangeKm: 5.0,
    };

    res = http.post(
      `${BASE_URL}/los/viewshed`,
      JSON.stringify(viewshedRequest),
      { headers: { "Content-Type": "application/json" } },
    );
    success = check(res, {
      "CalculateViewshed status 200": (r) => r.status === 200,
      "CalculateViewshed has response": (r) => r.body.length > 0,
      "CalculateViewshed response time < 2000ms": (r) =>
        r.timings.duration < 2000,
    });
    losLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);
  });

  group("SRTM API", () => {
    // Test SRTM Status endpoint
    let res = http.get(
      `${BASE_URL}/srtm/status?minLat=48&minLon=28&maxLat=52&maxLon=32`,
    );
    let success = check(res, {
      "SRTM Status status 200": (r) => r.status === 200,
      "SRTM Status has data": (r) => r.body.includes("exists"),
      "SRTM Status response time < 500ms": (r) => r.timings.duration < 500,
    });
    srtmLatency.add(res.timings.duration);
    success ? successCount.add(1) : failureCount.add(1);
    errorRate.add(!success);
    sleep(0.5);
  });
}
