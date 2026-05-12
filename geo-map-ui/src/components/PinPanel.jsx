import { useState } from "react";

export default function PinPanel({ pins, onAdd, onClear }) {
  const [lat, setLat] = useState("");
  const [lon, setLon] = useState("");
  const [label, setLabel] = useState("");

  const valid =
    lat !== "" && lon !== "" &&
    !isNaN(parseFloat(lat)) && !isNaN(parseFloat(lon));

  const commit = () => {
    if (!valid) return;
    onAdd({ lat: parseFloat(lat), lon: parseFloat(lon), label: label.trim() || undefined });
    setLat("");
    setLon("");
    setLabel("");
  };

  const onKey = (e) => { if (e.key === "Enter") commit(); };

  return (
    <div className="los-panel">
      <div className="los-panel-header">
        <span className="los-panel-title">📍 Pinpoints</span>
        <button className="los-clear-btn" onClick={onClear} title="Clear all pins">✕</button>
      </div>

      <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
        <label className="los-h-label">
          Latitude
          <input
            className="los-h-input" style={{ width: "100%" }}
            type="number" step="any" placeholder="e.g. 50.4501"
            value={lat} onChange={(e) => setLat(e.target.value)} onKeyDown={onKey}
          />
        </label>
        <label className="los-h-label">
          Longitude
          <input
            className="los-h-input" style={{ width: "100%" }}
            type="number" step="any" placeholder="e.g. 30.5234"
            value={lon} onChange={(e) => setLon(e.target.value)} onKeyDown={onKey}
          />
        </label>
        <label className="los-h-label">
          Label <span style={{ color: "#484f58" }}>(optional)</span>
          <input
            className="los-h-input" style={{ width: "100%" }}
            type="text" placeholder="e.g. HQ"
            value={label} onChange={(e) => setLabel(e.target.value)} onKeyDown={onKey}
          />
        </label>
        <button className="los-calc-btn" onClick={commit} disabled={!valid}>
          + Add Pin
        </button>
      </div>

      <div style={{ fontSize: "0.7rem", color: "#484f58", fontStyle: "italic" }}>
        or click the map to drop a numbered pin
      </div>

      {pins.length > 0 && (
        <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
          {pins.map((p, i) => (
            <div key={i} className="los-coord-row">
              <span className="los-coord-label" style={{ color: "#e74c3c" }}>
                📍 {p.label ?? i + 1}
              </span>
              <span className="los-coord-val">
                {p.lat.toFixed(5)}, {p.lon.toFixed(5)}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
