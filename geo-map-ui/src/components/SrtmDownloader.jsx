import { useState } from "react";

export default function SrtmDownloader({ onClose }) {
  const [minLat, setMinLat] = useState("");
  const [minLon, setMinLon] = useState("");
  const [maxLat, setMaxLat] = useState("");
  const [maxLon, setMaxLon] = useState("");
  const [log, setLog] = useState([]);
  const [busy, setBusy] = useState(false);
  const [done, setDone] = useState(false);

  const addLog = (msg, type = "info") =>
    setLog((l) => [...l, { msg, type }]);

  const download = async () => {
    const body = {
      minLat: parseFloat(minLat),
      minLon: parseFloat(minLon),
      maxLat: parseFloat(maxLat),
      maxLon: parseFloat(maxLon),
    };
    if (Object.values(body).some(isNaN)) {
      addLog("Fill in all four coordinates first.", "error");
      return;
    }

    setBusy(true);
    setDone(false);
    setLog([]);
    addLog("Checking required tiles…");

    try {
      // Check status first
      const q = new URLSearchParams(body).toString();
      const statusRes = await fetch(`/Srtm/status?${q}`);
      const tiles = await statusRes.json();

      const missing = tiles.filter((t) => !t.exists);
      const existing = tiles.filter((t) => t.exists);

      if (existing.length) addLog(`Already have: ${existing.map((t) => t.tile).join(", ")}`, "ok");
      if (!missing.length) {
        addLog("All tiles already downloaded! Ready for offline use.", "ok");
        setDone(true);
        setBusy(false);
        return;
      }

      addLog(`Downloading ${missing.length} tile(s): ${missing.map((t) => t.tile).join(", ")}`);
      addLog("This may take a minute — each tile is ~1–3 MB…");

      const dlRes = await fetch("/Srtm/download", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });
      if (!dlRes.ok) throw new Error(`HTTP ${dlRes.status}`);

      const results = await dlRes.json();
      results.forEach(({ tile, status }) => {
        if (status === "ok") addLog(`✔ ${tile} downloaded`, "ok");
        else if (status === "already_exists") addLog(`— ${tile} already exists`, "info");
        else if (status === "no_data_ocean") addLog(`~ ${tile} ocean/no-data (placeholder created)`, "warn");
        else addLog(`✘ ${tile}: ${status}`, "error");
      });

      const failed = results.filter((r) => r.status.startsWith("error"));
      if (failed.length === 0) {
        addLog("Done! Elevation data is ready. The app now works offline.", "ok");
        setDone(true);
      } else {
        addLog(`${failed.length} tile(s) failed. Check your internet connection.`, "error");
      }
    } catch (e) {
      addLog(`Error: ${e.message}`, "error");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="srtm-overlay">
      <div className="srtm-dialog">
        <div className="srtm-header">
          <span>Download Elevation Data (SRTM)</span>
          <button className="los-clear-btn" onClick={onClose}>✕</button>
        </div>

        <p className="srtm-info">
          Enter the bounding box of your area of interest. Tiles (~1–3 MB each)
          will be downloaded from a free public server and saved locally.
          After this you can use the app <strong>fully offline</strong>.
        </p>

        <div className="srtm-grid">
          <label>Min Lat<input className="los-h-input srtm-input" value={minLat} onChange={e => setMinLat(e.target.value)} placeholder="e.g. 49.5" /></label>
          <label>Min Lon<input className="los-h-input srtm-input" value={minLon} onChange={e => setMinLon(e.target.value)} placeholder="e.g. 29.5" /></label>
          <label>Max Lat<input className="los-h-input srtm-input" value={maxLat} onChange={e => setMaxLat(e.target.value)} placeholder="e.g. 51.5" /></label>
          <label>Max Lon<input className="los-h-input srtm-input" value={maxLon} onChange={e => setMaxLon(e.target.value)} placeholder="e.g. 31.5" /></label>
        </div>

        <button className="los-calc-btn srtm-btn" onClick={download} disabled={busy}>
          {busy ? "Downloading…" : "Download Tiles"}
        </button>

        {log.length > 0 && (
          <div className="srtm-log">
            {log.map((l, i) => (
              <div key={i} className={`srtm-log-line srtm-log--${l.type}`}>{l.msg}</div>
            ))}
          </div>
        )}

        {done && (
          <button className="los-calc-btn srtm-btn" onClick={onClose}>
            Close &amp; Start Using LOS
          </button>
        )}
      </div>
    </div>
  );
}
