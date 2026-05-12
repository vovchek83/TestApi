import { useEffect, useRef } from "react";

const PAD = { top: 12, right: 16, bottom: 28, left: 52 };

function lerp(v, inMin, inMax, outMin, outMax) {
  return outMin + ((v - inMin) / (inMax - inMin)) * (outMax - outMin);
}

export default function ProfileChart({ profile, txAmsl, rxAmsl }) {
  const canvasRef = useRef(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas || !profile?.length) return;

    const dpr = window.devicePixelRatio || 1;
    const W = canvas.clientWidth;
    const H = canvas.clientHeight;
    canvas.width = W * dpr;
    canvas.height = H * dpr;
    const ctx = canvas.getContext("2d");
    ctx.scale(dpr, dpr);

    const inner = {
      x: PAD.left,
      y: PAD.top,
      w: W - PAD.left - PAD.right,
      h: H - PAD.top - PAD.bottom,
    };

    const maxDist = profile[profile.length - 1].distanceKm;
    const allElev = profile.flatMap((p) => [
      p.terrainAmsl,
      p.beamAmsl + p.fresnelRadius,
    ]);
    const minE = Math.min(...allElev) - 20;
    const maxE = Math.max(...allElev) + 30;

    const xOf = (d) => lerp(d, 0, maxDist, inner.x, inner.x + inner.w);
    const yOf = (e) => lerp(e, minE, maxE, inner.y + inner.h, inner.y);

    // ── Background ───────────────────────────────────────────────────────────
    ctx.fillStyle = "#0d1117";
    ctx.fillRect(0, 0, W, H);

    // ── Grid ─────────────────────────────────────────────────────────────────
    ctx.strokeStyle = "#21262d";
    ctx.lineWidth = 0.5;
    const gridLines = 5;
    for (let i = 0; i <= gridLines; i++) {
      const e = lerp(i / gridLines, 0, 1, minE, maxE);
      const y = yOf(e);
      ctx.beginPath();
      ctx.moveTo(inner.x, y);
      ctx.lineTo(inner.x + inner.w, y);
      ctx.stroke();
    }

    // ── Fresnel zone envelope ─────────────────────────────────────────────────
    ctx.beginPath();
    profile.forEach((p, i) => {
      const x = xOf(p.distanceKm);
      const y = yOf(p.beamAmsl + p.fresnelRadius * 0.6);
      i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
    });
    for (let i = profile.length - 1; i >= 0; i--) {
      const p = profile[i];
      ctx.lineTo(xOf(p.distanceKm), yOf(p.beamAmsl - p.fresnelRadius * 0.6));
    }
    ctx.closePath();
    ctx.fillStyle = "rgba(46,204,113,0.08)";
    ctx.fill();
    ctx.strokeStyle = "rgba(46,204,113,0.25)";
    ctx.lineWidth = 0.8;
    ctx.stroke();

    // ── Terrain fill ─────────────────────────────────────────────────────────
    ctx.beginPath();
    profile.forEach((p, i) => {
      const x = xOf(p.distanceKm);
      const y = yOf(p.terrainAmsl);
      i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
    });
    ctx.lineTo(xOf(maxDist), yOf(minE));
    ctx.lineTo(xOf(0), yOf(minE));
    ctx.closePath();
    ctx.fillStyle = "rgba(101,93,80,0.55)";
    ctx.fill();
    ctx.strokeStyle = "#a09070";
    ctx.lineWidth = 1;
    ctx.stroke();

    // ── Earth curvature correction line (effective terrain) ──────────────────
    ctx.beginPath();
    profile.forEach((p, i) => {
      const x = xOf(p.distanceKm);
      const y = yOf(p.effectiveTerrainAmsl);
      i === 0 ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
    });
    ctx.strokeStyle = "rgba(230,180,50,0.5)";
    ctx.lineWidth = 1;
    ctx.setLineDash([4, 3]);
    ctx.stroke();
    ctx.setLineDash([]);

    // ── Beam line (clear = green, blocked = red) ──────────────────────────────
    let segStart = 0;
    const drawBeamSeg = (start, end) => {
      const blocked = profile[start].blocked;
      ctx.beginPath();
      for (let i = start; i < end; i++) {
        const p = profile[i];
        const x = xOf(p.distanceKm);
        const y = yOf(p.beamAmsl);
        i === start ? ctx.moveTo(x, y) : ctx.lineTo(x, y);
      }
      ctx.strokeStyle = blocked ? "#e74c3c" : "#2ecc71";
      ctx.lineWidth = 2;
      ctx.stroke();
    };

    for (let i = 1; i <= profile.length; i++) {
      const change = i === profile.length || profile[i].blocked !== profile[segStart].blocked;
      if (change) {
        drawBeamSeg(segStart, i);
        segStart = i;
      }
    }

    // ── Axes ─────────────────────────────────────────────────────────────────
    ctx.strokeStyle = "#30363d";
    ctx.lineWidth = 1;
    ctx.beginPath();
    ctx.moveTo(inner.x, inner.y);
    ctx.lineTo(inner.x, inner.y + inner.h);
    ctx.lineTo(inner.x + inner.w, inner.y + inner.h);
    ctx.stroke();

    ctx.fillStyle = "#8b949e";
    ctx.font = "10px ui-monospace, monospace";
    ctx.textAlign = "right";

    for (let i = 0; i <= gridLines; i++) {
      const e = Math.round(lerp(i / gridLines, minE, maxE, minE, maxE) / 10) * 10;
      const y = yOf(e);
      ctx.fillText(`${e}m`, inner.x - 4, y + 3);
    }

    ctx.textAlign = "center";
    const distTicks = 5;
    for (let i = 0; i <= distTicks; i++) {
      const d = (maxDist * i) / distTicks;
      ctx.fillText(`${d.toFixed(1)}km`, xOf(d), inner.y + inner.h + 16);
    }

    // ── TX / RX labels ────────────────────────────────────────────────────────
    ctx.font = "bold 11px sans-serif";
    ctx.fillStyle = "#e67e22";
    ctx.textAlign = "center";
    ctx.fillText("TX", xOf(0), yOf(txAmsl) - 8);
    ctx.fillStyle = "#2980b9";
    ctx.fillText("RX", xOf(maxDist), yOf(rxAmsl) - 8);

  }, [profile, txAmsl, rxAmsl]);

  if (!profile?.length) return null;

  return (
    <div className="profile-wrap">
      <canvas ref={canvasRef} className="profile-canvas" />
    </div>
  );
}
