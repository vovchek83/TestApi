# geo-map-ui

React + OpenLayers frontend for testing the GeoTesterApi (.NET backend). Displays GeoJSON responses on an interactive map and provides LOS (Line of Sight), viewshed, and pinpoint tools.

## Stack

- **React 18** + **Vite 5** (no TypeScript, no test suite)
- **OpenLayers 10** for map rendering
- Backend: ASP.NET Core at `http://localhost:5297` (must be running for API buttons to work)
- Vite dev proxy routes `/Geo`, `/Los`, `/tiles`, `/Srtm` → `localhost:5297`

## Commands

```bash
npm run dev      # dev server on :3000 (or $PORT)
npm run build    # production build → dist/
npm run preview  # serve dist/
```

## Project structure

```
src/
  services/
    geoApi.js           # ENDPOINTS array + fetchGeoData(ep) — pure fetch, no React
  components/
    MapView.jsx         # OpenLayers map — all OL logic lives here
    Toolbar.jsx         # Header layout shell (brand + children)
    EndpointButtons.jsx # Nav buttons for geo API endpoints
    StatusBar.jsx       # Spinner + status text
    LosPanel.jsx        # LOS / viewshed side panel
    PinPanel.jsx        # Pinpoint drop-marker side panel
    ProfileChart.jsx    # Elevation profile canvas chart
    SrtmDownloader.jsx  # SRTM tile download overlay dialog
  App.jsx               # State + wiring — thin orchestrator only
  App.css               # All styles (single file, no CSS modules)
  index.css             # Global reset + body
  main.jsx              # ReactDOM entry point
```

## MapView imperative API

`MapView` uses `forwardRef` + `useImperativeHandle`. Parent calls methods via `mapRef.current`:

| Method | Description |
|---|---|
| `displayFeatures(geojson, styleKey)` | Render GeoJSON polygons with numbered drop labels. Returns feature count. |
| `displayLos(tx, rx, profile)` | Draw TX/RX markers and segmented LOS line (clear=green, blocked=red). |
| `clearLos()` | Remove all LOS overlay features. |
| `displayViewshed(tx, polygonCoords)` | Draw TX marker + viewshed polygon. |
| `addPinpoint({ lat, lon, label? })` | Drop a red SVG pin at the location. Auto-numbers if no label. Returns pin count. |
| `clearPinpoints()` | Remove all pins and reset counter. |

MapView layers (bottom → top):
1. Tile layer (OSM online or local XYZ)
2. `polyLayer` — GeoJSON polygon features (zIndex default)
3. `losLayer` — LOS/viewshed overlay (zIndex 10)
4. `pinLayer` — drop pin markers (zIndex 20)

## Adding a new geo endpoint

1. Add an entry to `ENDPOINTS` in `src/services/geoApi.js`
2. Add a style entry to `POLY_STYLES` in `src/components/MapView.jsx`
3. Add CSS for `.ep-btn--{id}` and `.ep-btn--{id}.ep-btn--active` in `src/App.css`

For POST endpoints that need a source body, add `sourceUrl` to the endpoint descriptor — `fetchGeoData` handles the pre-fetch automatically.

## Map click modes

Map clicks are routed in `App.jsx → handleMapClick`:
- `pinMode && !losMode` → drops a pin via `addPinpoint`
- `losMode` → LOS point placement state machine (`tx` → `rx` → done)

Both modes set `cursor: crosshair` on the map viewport. Only one click-mode is active at a time.

## Tile sources

- **OSM online** (default): standard OpenStreetMap
- **Local tiles**: XYZ from `/tiles/{z}/{x}/{y}.png` served by the backend — toggled via the "🌐 OSM Online / 🗺 Local Tiles" button

## CSS conventions

All styles live in `src/App.css`. Class naming:
- `.ep-btn--{endpointId}` — per-endpoint button accent color
- `.ep-btn--active` — active/selected state
- `.los-*` — LOS panel and related UI (reused by PinPanel)
- `.pin-toggle-btn` — pin mode toolbar button

No CSS modules, no Tailwind. Keep new styles in `App.css`.

## Key constraints

- No TypeScript — plain JSX throughout
- No test suite — verify changes by running `npm run dev` and clicking through the UI
- OL styles (`POLY_STYLES`) are module-level constants, not recreated per render
- Per-feature label styles (circle+number on polygons, pin icons) are built into a `styleCache` Map inside `displayFeatures` — avoids allocating Style objects on every OL render tick
- SVG pin icons are generated as `data:image/svg+xml` URLs with the label embedded — no external assets needed
