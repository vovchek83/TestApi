import { useState, useCallback } from "react";

const DEFAULT_FREQ = 900;
const DEFAULT_H = 10;

function CoordRow({ label, color, point, heightAgl, onHeightChange, hint }) {
  return (
    <div className="los-coord-row">
      <span className="los-coord-label" style={{ color }}>{label}</span>
      <span className="los-coord-val">
        {point
          ? `${point.lat.toFixed(5)}, ${point.lon.toFixed(5)}`
          : <em className="los-hint">{hint}</em>}
      </span>
      <label className="los-h-label">
        h AGL&nbsp;
        <input
          type="number"
          className="los-h-input"
          value={heightAgl}
          min={0}
          max={9999}
          step={1}
          onChange={(e) => onHeightChange(Number(e.target.value))}
        />
        &nbsp;m
      </label>
    </div>
  );
}

export default function LosPanel({ tx, rx, onClear, onResult, onViewshedResult }) {
  const [txH, setTxH] = useState(DEFAULT_H);
  const [rxH, setRxH] = useState(DEFAULT_H);
  const [freq, setFreq] = useState(DEFAULT_FREQ);
  const [maxRange, setMaxRange] = useState(20);
  const [loading, setLoading] = useState(false);
  const [mode, setMode] = useState("los"); // "los" | "viewshed"
  const [result, setResult] = useState(null);
  const [viewshedResult, setViewshedResult] = useState(null);
  const [error, setError] = useState(null);

  const calculate = useCallback(async () => {
    if (!tx || !rx) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const res = await fetch("/Los/calculate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          tx: { lat: tx.lat, lon: tx.lon, heightAgl: txH },
          rx: { lat: rx.lat, lon: rx.lon, heightAgl: rxH },
          frequencyMhz: freq,
          samples: 400,
        }),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setResult(data);
      onResult?.(data);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [tx, rx, txH, rxH, freq, onResult]);

  const calculateViewshed = useCallback(async () => {
    if (!tx) return;
    setLoading(true);
    setError(null);
    setViewshedResult(null);
    try {
      const res = await fetch("/Los/viewshed", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          tx: { lat: tx.lat, lon: tx.lon, heightAgl: txH },
          rxHeightAgl: 2.0,
          maxRangeKm: maxRange,
          frequencyMhz: freq,
          angularResolutionDeg: 1.0,
          samplesPerRay: 300,
        }),
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setViewshedResult(data);
      onViewshedResult?.(data);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [tx, txH, maxRange, freq, onViewshedResult]);

  const handleClear = () => {
    setResult(null);
    setViewshedResult(null);
    setError(null);
    onClear?.();
  };

  return (
    <div className="los-panel">
      <div className="los-panel-header">
        <span className="los-panel-title">📡 Radio LOS</span>
        <button className="los-clear-btn" onClick={handleClear} title="Clear">✕</button>
      </div>

      {/* Mode tabs */}
      <div className="los-tabs">
        <button
          className={`los-tab ${mode === "los" ? "los-tab--active" : ""}`}
          onClick={() => setMode("los")}
        >Point LOS</button>
        <button
          className={`los-tab ${mode === "viewshed" ? "los-tab--active" : ""}`}
          onClick={() => setMode("viewshed")}
        >🗺 Viewshed</button>
      </div>

      <CoordRow
        label="TX"
        color="#e67e22"
        point={tx}
        heightAgl={txH}
        onHeightChange={setTxH}
        hint="click map to place"
      />

      {mode === "los" && (
        <CoordRow
          label="RX"
          color="#2980b9"
          point={rx}
          heightAgl={rxH}
          onHeightChange={setRxH}
          hint={tx ? "click map to place" : "place TX first"}
        />
      )}

      <div className="los-freq-row">
        <label>
          Freq&nbsp;
          <input type="number" className="los-h-input" value={freq} min={1} max={100000} step={10}
            onChange={(e) => setFreq(Number(e.target.value))} />
          &nbsp;MHz
        </label>
        {mode === "viewshed" && (
          <label style={{ marginLeft: 12 }}>
            Range&nbsp;
            <input type="number" className="los-h-input" value={maxRange} min={1} max={100} step={1}
              onChange={(e) => setMaxRange(Number(e.target.value))} />
            &nbsp;km
          </label>
        )}
      </div>

      {mode === "los" ? (
        <button className="los-calc-btn" disabled={!tx || !rx || loading} onClick={calculate}>
          {loading ? "Calculating…" : "Calculate LOS"}
        </button>
      ) : (
        <button className="los-calc-btn los-calc-btn--viewshed" disabled={!tx || loading} onClick={calculateViewshed}>
          {loading ? "Computing viewshed…" : "Calculate Viewshed"}
        </button>
      )}

      {error && <div className="los-result los-result--error">Error: {error}</div>}

      {mode === "los" && result && (
        <div className={`los-result ${result.isLos ? "los-result--ok" : "los-result--blocked"}`}>
          <strong>{result.isLos ? "✔ LOS CLEAR" : "✘ BLOCKED"}</strong>
          <span>Distance: {result.distanceKm.toFixed(2)} km</span>
          <span>TX: {result.txAmsl.toFixed(0)} m AMSL</span>
          <span>RX: {result.rxAmsl.toFixed(0)} m AMSL</span>
          {!result.isLos && result.firstObstruction && (
            <span>
              First block: {result.firstObstruction.distanceKm.toFixed(2)} km
              ({result.firstObstruction.effectiveTerrainAmsl.toFixed(0)} m)
            </span>
          )}
        </div>
      )}

      {mode === "viewshed" && viewshedResult && (
        <div className="los-result los-result--ok">
          <strong>✔ Viewshed computed</strong>
          <span>TX: {viewshedResult.txAmsl.toFixed(0)} m AMSL</span>
          <span>Coverage: ~{viewshedResult.coverageAreaKm2.toFixed(1)} km²</span>
          <span>{viewshedResult.rayCount} rays @ 1° resolution</span>
        </div>
      )}
    </div>
  );
}
