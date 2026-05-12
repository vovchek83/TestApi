import {
  useRef,
  useEffect,
  useCallback,
  forwardRef,
  useImperativeHandle,
} from "react";
import Map from "ol/Map";
import View from "ol/View";
import TileLayer from "ol/layer/Tile";
import VectorLayer from "ol/layer/Vector";
import VectorSource from "ol/source/Vector";
import XYZ from "ol/source/XYZ";
import OSM from "ol/source/OSM";
import GeoJSON from "ol/format/GeoJSON";
import Feature from "ol/Feature";
import Point from "ol/geom/Point";
import LineString from "ol/geom/LineString";
import Polygon from "ol/geom/Polygon";
import { fromLonLat, toLonLat } from "ol/proj";
import { Fill, Stroke, Style, Circle as CircleStyle, Text } from "ol/style";
import Icon from "ol/style/Icon";
import "ol/ol.css";

// ── Polygon display styles (existing feature) ────────────────────────────────
const POLY_STYLES = {
  polygon: new Style({ fill: new Fill({ color: "rgba(52,152,219,.25)" }), stroke: new Stroke({ color: "#3498db", width: 2.5 }) }),
  multipolygon: new Style({ fill: new Fill({ color: "rgba(155,89,182,.25)" }), stroke: new Stroke({ color: "#9b59b6", width: 2.5 }) }),
  donat: new Style({ fill: new Fill({ color: "rgba(0,24,241,.25)" }), stroke: new Stroke({ color: "#b301fa", width: 2.5 }) }),
  union: new Style({ fill: new Fill({ color: "rgba(46,213,115,.25)" }), stroke: new Stroke({ color: "#2ed573", width: 2.5 }) }),
  "union-polygon": new Style({ fill: new Fill({ color: "rgba(255,165,0,.25)" }), stroke: new Stroke({ color: "#ff8c00", width: 2.5 }) }),
};

// ── LOS overlay styles ───────────────────────────────────────────────────────
const LOS_CLEAR_STYLE = new Style({ stroke: new Stroke({ color: "#2ecc71", width: 3 }) });
const LOS_BLOCKED_STYLE = new Style({ stroke: new Stroke({ color: "#e74c3c", width: 3 }) });

const VIEWSHED_STYLE = new Style({
  fill: new Fill({ color: "rgba(46, 204, 113, 0.18)" }),
  stroke: new Stroke({ color: "#2ecc71", width: 2, lineDash: [6, 3] }),
});

function markerStyle(label, color) {
  return new Style({
    image: new CircleStyle({
      radius: 10,
      fill: new Fill({ color }),
      stroke: new Stroke({ color: "#fff", width: 2 }),
    }),
    text: new Text({
      text: label,
      fill: new Fill({ color: "#fff" }),
      font: "bold 11px sans-serif",
      offsetY: 1,
    }),
  });
}

const TX_STYLE = markerStyle("TX", "#e67e22");
const RX_STYLE = markerStyle("RX", "#2980b9");

function createPinSvg(label, color) {
  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" width="24" height="34" viewBox="0 0 24 34">` +
    `<path d="M12 0C5.373 0 0 5.373 0 12c0 6.627 12 22 12 22S24 18.627 24 12C24 5.373 18.627 0 12 0z" fill="${color}"/>` +
    `<circle cx="12" cy="12" r="6" fill="#fff" fill-opacity="0.95"/>` +
    `<text x="12" y="16" text-anchor="middle" font-size="9" font-weight="bold" font-family="sans-serif" fill="${color}">${label}</text>` +
    `</svg>`;
  return "data:image/svg+xml;charset=utf-8," + encodeURIComponent(svg);
}

function pinStyle(label) {
  return new Style({
    image: new Icon({
      src: createPinSvg(label, "#e74c3c"),
      anchor: [0.5, 1.0],
      anchorXUnits: "fraction",
      anchorYUnits: "fraction",
    }),
  });
}

function getInteriorPoint(geom) {
  if (geom.getType() === "MultiPolygon")
    return geom.getPolygons().reduce((a, b) => (a.getArea() > b.getArea() ? a : b)).getInteriorPoint();
  return geom.getInteriorPoint();
}

const MapView = forwardRef(function MapView({ losMode, pinMode, onMapClick, useLocalTiles }, ref) {
  const containerRef = useRef(null);
  const mapRef = useRef(null);
  const polySource = useRef(null);
  const polyLayer = useRef(null);
  const losSource = useRef(null);
  const pinSource = useRef(null);
  const pinCount = useRef(0);
  const tileLayerRef = useRef(null);

  // Swap tile source when useLocalTiles prop changes
  useEffect(() => {
    if (!tileLayerRef.current) return;
    tileLayerRef.current.setSource(
      useLocalTiles
        ? new XYZ({ url: "/tiles/{z}/{x}/{y}.png", crossOrigin: "anonymous" })
        : new OSM()
    );
  }, [useLocalTiles]);

  useEffect(() => {
    polySource.current = new VectorSource();
    polyLayer.current = new VectorLayer({ source: polySource.current });

    losSource.current = new VectorSource();
    const losLayer = new VectorLayer({ source: losSource.current, zIndex: 10 });

    pinSource.current = new VectorSource();
    const pinLayer = new VectorLayer({ source: pinSource.current, zIndex: 20 });

    // Default: OSM online. Switch to local tiles via useLocalTiles prop.
    tileLayerRef.current = new TileLayer({ source: new OSM() });

    mapRef.current = new Map({
      target: containerRef.current,
      layers: [tileLayerRef.current, polyLayer.current, losLayer, pinLayer],
      view: new View({ center: fromLonLat([30.52, 50.44]), zoom: 10 }),
    });

    return () => mapRef.current.setTarget(null);
  }, []);

  // Click handler for LOS point placement
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;

    const handler = (e) => {
      if (!losMode && !pinMode) return;
      const [lon, lat] = toLonLat(e.coordinate);
      onMapClick?.({ lat, lon });
    };

    map.on("click", handler);
    return () => map.un("click", handler);
  }, [losMode, pinMode, onMapClick]);

  // Cursor style in LOS mode
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    map.getViewport().style.cursor = (losMode || pinMode) ? "crosshair" : "default";
  }, [losMode, pinMode]);

  // ── Exposed API ────────────────────────────────────────────────────────────
  const displayFeatures = useCallback((geojson, styleKey) => {
    polySource.current.clear();
    const features = new GeoJSON().readFeatures(geojson, { featureProjection: "EPSG:3857" });
    polySource.current.addFeatures(features);

    const baseStyle = POLY_STYLES[styleKey];
    const accentColor = baseStyle.getStroke().getColor();
    const styleCache = new Map();

    features.forEach((f, i) => {
      styleCache.set(f, [
        baseStyle,
        new Style({
          geometry: getInteriorPoint(f.getGeometry()),
          image: new CircleStyle({ radius: 14, fill: new Fill({ color: "#c7d2e2" }), stroke: new Stroke({ color: accentColor, width: 2 }) }),
          text: new Text({ text: String(i + 1), fill: new Fill({ color: "#242121" }), font: "bold 12px ui-monospace,monospace" }),
        }),
      ]);
    });

    polyLayer.current.setStyle((feature) => styleCache.get(feature) ?? baseStyle);
    const extent = polySource.current.getExtent();
    mapRef.current.getView().fit(extent, { padding: [100, 100, 100, 100], duration: 700 });
    return features.length;
  }, []);

  const displayLos = useCallback((tx, rx, profile) => {
    losSource.current.clear();
    if (!tx || !rx) return;

    // TX / RX markers
    const txFeature = new Feature({ geometry: new Point(fromLonLat([tx.lon, tx.lat])) });
    txFeature.setStyle(TX_STYLE);

    const rxFeature = new Feature({ geometry: new Point(fromLonLat([rx.lon, rx.lat])) });
    rxFeature.setStyle(RX_STYLE);

    losSource.current.addFeatures([txFeature, rxFeature]);

    if (!profile || profile.length === 0) return;

    // Split profile into contiguous clear / blocked segments, draw each as a line
    let segStart = 0;
    for (let i = 1; i <= profile.length; i++) {
      const end = i === profile.length || profile[i].blocked !== profile[segStart].blocked;
      if (!end) continue;

      const coords = profile.slice(segStart, i).map((p) => fromLonLat([p.lon, p.lat]));
      const line = new Feature({ geometry: new LineString(coords) });
      line.setStyle(profile[segStart].blocked ? LOS_BLOCKED_STYLE : LOS_CLEAR_STYLE);
      losSource.current.addFeature(line);
      segStart = i;
    }

    // Fit view to TX→RX extent
    const extent = losSource.current.getExtent();
    mapRef.current.getView().fit(extent, { padding: [80, 80, 80, 80], duration: 600 });
  }, []);

  const clearLos = useCallback(() => losSource.current?.clear(), []);

  const addPinpoint = useCallback(({ lat, lon, label }) => {
    pinCount.current += 1;
    const feature = new Feature({ geometry: new Point(fromLonLat([lon, lat])) });
    feature.setStyle(pinStyle(label ?? String(pinCount.current)));
    pinSource.current.addFeature(feature);
    return pinCount.current;
  }, []);

  const clearPinpoints = useCallback(() => {
    pinSource.current?.clear();
    pinCount.current = 0;
  }, []);

  // polygonCoords: [[lon, lat], ...] closed ring from the viewshed API
  const displayViewshed = useCallback((tx, polygonCoords) => {
    losSource.current.clear();

    // TX marker
    if (tx) {
      const txFeature = new Feature({ geometry: new Point(fromLonLat([tx.lon, tx.lat])) });
      txFeature.setStyle(TX_STYLE);
      losSource.current.addFeature(txFeature);
    }

    if (!polygonCoords?.length) return;

    const ring = polygonCoords.map(([lon, lat]) => fromLonLat([lon, lat]));
    const poly = new Feature({ geometry: new Polygon([ring]) });
    poly.setStyle(VIEWSHED_STYLE);
    losSource.current.addFeature(poly);

    const extent = losSource.current.getExtent();
    mapRef.current.getView().fit(extent, { padding: [60, 60, 60, 60], duration: 600 });
  }, []);

  useImperativeHandle(ref, () => ({ displayFeatures, displayLos, clearLos, displayViewshed, addPinpoint, clearPinpoints }), [
    displayFeatures, displayLos, clearLos, displayViewshed, addPinpoint, clearPinpoints,
  ]);

  return <div className="map-wrap" ref={containerRef} />;
});

export default MapView;
