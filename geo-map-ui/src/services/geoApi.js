export const ENDPOINTS = [
  {
    id: "polygon",
    label: "GET /Geo/polygon",
    method: "GET",
    url: "/Geo/polygon",
  },
  {
    id: "multipolygon",
    label: "GET /Geo/multipolygon",
    method: "GET",
    url: "/Geo/multipolygon",
  },
  { id: "donat", label: "GET Geo/donut", method: "GET", url: "Geo/donut" },
  {
    id: "union",
    label: "POST /Geo/union",
    method: "POST",
    url: "/Geo/union",
    sourceUrl: "/Geo/multipolygon",
  },
  {
    id: "union-polygon",
    label: "GET /Geo/union-polygon",
    method: "GET",
    url: "/Geo/union-polygon",
  },
];

export async function fetchGeoData(ep) {
  let body;
  if (ep.method === "POST" && ep.sourceUrl) {
    const srcRes = await fetch(ep.sourceUrl);
    if (!srcRes.ok)
      throw new Error(`Source fetch failed: HTTP ${srcRes.status}`);
    body = await srcRes.json();
  }

  const res = await fetch(ep.url, {
    method: ep.method,
    headers:
      ep.method === "POST" ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`HTTP ${res.status} ${res.statusText}`);
  return res.json();
}
