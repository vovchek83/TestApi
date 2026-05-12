import { useRef, useState, useCallback } from "react";
import { ENDPOINTS, fetchGeoData } from "./services/geoApi.js";
import MapView from "./components/MapView.jsx";
import Toolbar from "./components/Toolbar.jsx";
import EndpointButtons from "./components/EndpointButtons.jsx";
import StatusBar from "./components/StatusBar.jsx";
import LosPanel from "./components/LosPanel.jsx";
import PinPanel from "./components/PinPanel.jsx";
import ProfileChart from "./components/ProfileChart.jsx";
import SrtmDownloader from "./components/SrtmDownloader.jsx";
import "./App.css";

// LOS click state machine: "tx" → place TX, "rx" → place RX, null → done
const CLICK_STEPS = { tx: "rx", rx: null };

export default function App() {
  const mapRef = useRef(null);

  // ── Geo API mode ───────────────────────────────────────────────────────────
  const [active, setActive] = useState(null);
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState({ text: "Select an endpoint or switch to LOS mode", type: "idle" });

  // ── Pin mode ───────────────────────────────────────────────────────────────
  const [pinMode, setPinMode] = useState(false);
  const [pins, setPins] = useState([]);

  const togglePinMode = useCallback(() => setPinMode((v) => !v), []);

  const handleAddPin = useCallback(({ lat, lon, label }) => {
    mapRef.current?.addPinpoint({ lat, lon, label });
    setPins((prev) => [...prev, { lat, lon, label }]);
  }, []);

  const handleClearPins = useCallback(() => {
    mapRef.current?.clearPinpoints();
    setPins([]);
  }, []);

  // ── LOS mode ───────────────────────────────────────────────────────────────
  const [losMode, setLosMode] = useState(false);
  const [showSrtmDl, setShowSrtmDl] = useState(false);
  const [useLocalTiles, setUseLocalTiles] = useState(false);
  const [clickStep, setClickStep] = useState("tx"); // "tx" | "rx" | null
  const [tx, setTx] = useState(null);
  const [rx, setRx] = useState(null);
  const [losResult, setLosResult] = useState(null);

  // ── Geo API handler ────────────────────────────────────────────────────────
  const handleSelect = useCallback(async (ep) => {
    setLoading(true);
    setActive(ep.id);
    setStatus({ text: `Calling ${ep.method} ${ep.url}…`, type: "loading" });
    try {
      const data = await fetchGeoData(ep);
      const count = mapRef.current.displayFeatures(data, ep.id);
      setStatus({ text: `${ep.label} — ${count} feature(s) loaded`, type: "ok" });
    } catch (err) {
      setStatus({ text: `Error: ${err.message}`, type: "error" });
    } finally {
      setLoading(false);
    }
  }, []);

  // ── LOS mode toggle ────────────────────────────────────────────────────────
  const toggleLos = useCallback(() => {
    setLosMode((v) => {
      if (!v) {
        // Entering LOS mode
        setClickStep("tx");
        setTx(null);
        setRx(null);
        setLosResult(null);
        mapRef.current?.clearLos();
      }
      return !v;
    });
  }, []);

  // ── Map click handler (LOS point placement) ────────────────────────────────
  const handleMapClick = useCallback(({ lat, lon }) => {
    if (pinMode && !losMode) {
      handleAddPin({ lat, lon });
      return;
    }
    if (clickStep === "tx") {
      setTx({ lat, lon });
      setRx(null);
      setLosResult(null);
      mapRef.current?.displayLos({ lat, lon }, null, null);
      setClickStep("rx");
    } else if (clickStep === "rx") {
      setRx({ lat, lon });
      mapRef.current?.displayLos(tx, { lat, lon }, null);
      setClickStep(null);
    }
  }, [clickStep, tx, pinMode, losMode, handleAddPin]);

  // ── LOS result handler ─────────────────────────────────────────────────────
  const handleLosResult = useCallback((result) => {
    setLosResult(result);
    mapRef.current?.displayLos(tx, rx, result.profile);
  }, [tx, rx]);

  // ── Viewshed result handler ────────────────────────────────────────────────
  const handleViewshedResult = useCallback((result) => {
    setLosResult(null); // hide profile chart while showing viewshed
    mapRef.current?.displayViewshed(tx, result.polygonCoords);
  }, [tx]);

  // ── LOS clear ─────────────────────────────────────────────────────────────
  const handleLosClear = useCallback(() => {
    setTx(null);
    setRx(null);
    setLosResult(null);
    setClickStep("tx");
    mapRef.current?.clearLos();
  }, []);

  return (
    <div className="app">
      <Toolbar brand="GeoTesterApi">
        {!losMode && (
          <EndpointButtons
            endpoints={ENDPOINTS}
            active={active}
            loading={loading}
            onSelect={handleSelect}
          />
        )}
        {losMode && (
          <span className="los-step-hint">
            {clickStep === "tx" && "Click map to place TX (transmitter)"}
            {clickStep === "rx" && "Click map to place RX (receiver)"}
            {clickStep === null && "Set heights then click Calculate"}
          </span>
        )}
        <button
          className={`ep-btn pin-toggle-btn ${pinMode ? "ep-btn--active" : ""}`}
          onClick={togglePinMode}
        >
          📍 {pinMode ? "Exit Pins" : "Pins"}
        </button>
        <button
          className={`ep-btn los-toggle-btn ${losMode ? "ep-btn--active" : ""}`}
          onClick={toggleLos}
        >
          📡 {losMode ? "Exit LOS" : "LOS Mode"}
        </button>
        <button
          className={`ep-btn map-src-btn ${useLocalTiles ? "map-src-btn--local" : ""}`}
          onClick={() => setUseLocalTiles(v => !v)}
          title={useLocalTiles ? "Using local tiles — click to switch to OSM online" : "Using OSM online — click to switch to local tiles"}
        >
          {useLocalTiles ? "🗺 Local Tiles" : "🌐 OSM Online"}
        </button>
        <button
          className="ep-btn srtm-dl-btn"
          onClick={() => setShowSrtmDl(true)}
          title="Download SRTM elevation tiles for offline use"
        >
          ⬇ Elevation Data
        </button>
        {!losMode && <StatusBar status={status} loading={loading} />}
      </Toolbar>

      <div className="content">
        <MapView
          ref={mapRef}
          losMode={losMode}
          pinMode={pinMode}
          onMapClick={handleMapClick}
          useLocalTiles={useLocalTiles}
        />

        {pinMode && (
          <PinPanel
            pins={pins}
            onAdd={handleAddPin}
            onClear={handleClearPins}
          />
        )}
        {losMode && (
          <LosPanel
            tx={tx}
            rx={rx}
            onClear={handleLosClear}
            onResult={handleLosResult}
            onViewshedResult={handleViewshedResult}
          />
        )}
      </div>

      {showSrtmDl && <SrtmDownloader onClose={() => setShowSrtmDl(false)} />}

      {losMode && losResult && (
        <ProfileChart
          profile={losResult.profile}
          txAmsl={losResult.txAmsl}
          rxAmsl={losResult.rxAmsl}
        />
      )}
    </div>
  );
}
