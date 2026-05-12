using System.Text.RegularExpressions;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using GeoToolkit.Interfaces;
using NetTopologySuite.IO;
using NtsGeometry = NetTopologySuite.Geometries;

namespace GeoTesterApi.Services;

public class PolygonService(IGeometryOperationsService operationsService, IGeoUnionService geoUnionService) : IPolygonService
{
    public FeatureCollection CreatePolygon()
    {
        var polygon = new Polygon(new[]
        {
            new LineString(new[]
            {
                new Position(50.4501, 30.5234),
                new Position(50.4501, 30.5634),
                new Position(50.4801, 30.5634),
                new Position(50.4801, 30.5234),
                new Position(50.4501, 30.5234),
            })
        });

        var feature = new Feature(polygon, new Dictionary<string, object>
        {
            { "name", "Sample Polygon" },
            { "description", "A simple rectangular polygon near Kyiv" }
        });

        return new FeatureCollection(new List<Feature> { feature });
    }

    public FeatureCollection CreateMultiPolygon()
    {


        var multiPolygon = new MultiPolygon(new[]
        {
    new Polygon(new[]
    {
        new LineString(new[]
        {
            new Position(50.4501, 30.5234),
            new Position(50.4501, 30.5634),
            new Position(50.4801, 30.5634),
            new Position(50.4801, 30.5234),
            new Position(50.4501, 30.5234),
        })
    }),
    new Polygon(new[]
    {
        new LineString(new[]
        {
            new Position(50.4601, 30.5034),  // was 50.3901, 30.4634
            new Position(50.4601, 30.5434),  // was 50.3901, 30.5034
            new Position(50.4901, 30.5434),  // was 50.4201, 30.5034
            new Position(50.4901, 30.5034),  // was 50.4201, 30.4634
            new Position(50.4601, 30.5034),  // was 50.3901, 30.4634
        })
    })
});
        var feature = new Feature(multiPolygon, new Dictionary<string, object>
        {
            { "name", "Sample MultiPolygon" },
            { "description", "Two rectangular polygons near Kyiv" }
        });
        return geoUnionService.Union(new FeatureCollection(new List<Feature> { feature }));
     
    }

    public FeatureCollection CreateDonutPolygon()
    {
        const double centerLat = 50.4651;
        const double centerLon = 30.5434;

        var outer = new LineString(CircleRing(centerLat, centerLon, outerRadius: 0.02));
        var hole  = new LineString(CircleRing(centerLat, centerLon, outerRadius: 0.008, clockwise: true));

        var donut = new Polygon(new[] { outer, hole });

        var feature = new Feature(donut, new Dictionary<string, object>
        {
            { "name", "Donut Polygon" },
            { "description", "A circular polygon with a circular hole near Kyiv" }
        });

        return new FeatureCollection(new List<Feature> { feature });
    }

    public FeatureCollection CreateMultiPolygonFromWkt()
    {
        var wktPath = Path.Combine(AppContext.BaseDirectory, "polygon.wkt");
        var wkt = Regex.Replace(File.ReadAllText(wktPath), @"[^\x09\x0A\x0D\x20-\x7E]", "");

       var feature1 = operationsService.FromWkt(wkt);

        //var reader = new WKTReader();
        //var geometry = reader.Read(wkt);

        //var ntsMultiPoly = geometry switch
        //{
        //    NtsGeometry.MultiPolygon mp       => mp,
        //    NtsGeometry.GeometryCollection gc => (NtsGeometry.MultiPolygon)gc.GetGeometryN(0),
        //    _ => throw new InvalidOperationException($"Unexpected geometry type: {geometry.GeometryType}")
        //};

        //var polygons = ntsMultiPoly.Geometries
        //    .Cast<NtsGeometry.Polygon>()
        //    .Select(ntsPoly =>
        //    {
        //        var rings = new List<LineString>();
        //        rings.Add(new LineString(RingPositions(ntsPoly.ExteriorRing)));
        //        foreach (var hole in ntsPoly.InteriorRings)
        //            rings.Add(new LineString(RingPositions(hole)));
        //        return new Polygon(rings);
        //    })
        //    .ToList();

        //var multiPolygon = new MultiPolygon(polygons);
        //var feature = new Feature(multiPolygon, new Dictionary<string, object>
        //{
        //    { "name", "WKT MultiPolygon" },
        //    { "source", "polygon.wkt" }
        //});


        return geoUnionService.Union(new FeatureCollection(new List<Feature> { feature1 }));
        return new FeatureCollection(new List<Feature> { feature1 });
    }

    public FeatureCollection CreateUnionPolygon()
    {
        var factory = NtsGeometry.GeometryFactory.Default;

        var poly1 = factory.CreatePolygon(new[]
        {
            new NtsGeometry.Coordinate(30.5234, 50.4501),
            new NtsGeometry.Coordinate(30.5634, 50.4501),
            new NtsGeometry.Coordinate(30.5634, 50.4801),
            new NtsGeometry.Coordinate(30.5234, 50.4801),
            new NtsGeometry.Coordinate(30.5234, 50.4501),
        });

        var poly2 = factory.CreatePolygon(new[]
        {
            new NtsGeometry.Coordinate(30.5034, 50.4601),
            new NtsGeometry.Coordinate(30.5434, 50.4601),
            new NtsGeometry.Coordinate(30.5434, 50.4901),
            new NtsGeometry.Coordinate(30.5034, 50.4901),
            new NtsGeometry.Coordinate(30.5034, 50.4601),
        });

        var unionResult = poly1.Union(poly2);

        var geoJsonGeometry = unionResult switch
        {
            NtsGeometry.Polygon p     => (IGeometryObject)NtsPolygonToGeoJson(p),
            NtsGeometry.MultiPolygon mp => NtsMultiPolygonToGeoJson(mp),
            _ => throw new InvalidOperationException($"Unexpected union geometry type: {unionResult.GeometryType}")
        };

        var feature = new Feature(geoJsonGeometry, new Dictionary<string, object>
        {
            { "name", "Union Polygon" },
            { "description", "Geometric union of two overlapping polygons near Kyiv" }
        });

        return new FeatureCollection(new List<Feature> { feature });
    }

    private static Polygon NtsPolygonToGeoJson(NtsGeometry.Polygon p)
    {
        var rings = new List<LineString> { new LineString(RingPositions(p.ExteriorRing)) };
        rings.AddRange(p.InteriorRings.Select(h => new LineString(RingPositions(h))));
        return new Polygon(rings);
    }

    private static MultiPolygon NtsMultiPolygonToGeoJson(NtsGeometry.MultiPolygon mp) =>
        new MultiPolygon(mp.Geometries.Cast<NtsGeometry.Polygon>().Select(NtsPolygonToGeoJson).ToList());

    private static IPosition[] RingPositions(NtsGeometry.LineString ring) =>
        ring.Coordinates.Select(c => (IPosition)new Position(c.Y, c.X)).ToArray();

    // Generates a closed ring of points approximating a circle.
    // lonScale corrects for longitude compression at higher latitudes so the
    // shape appears round on a projected map.
    private static IPosition[] CircleRing(double centerLat, double centerLon,
        double outerRadius, int points = 64, bool clockwise = false)
    {
        double lonScale = 1.0 / Math.Cos(centerLat * Math.PI / 180.0);
        double direction = clockwise ? -1 : 1;
        var ring = new IPosition[points + 1];
        for (int i = 0; i <= points; i++)
        {
            double angle = direction * 2 * Math.PI * i / points;
            ring[i] = new Position(
                centerLat + outerRadius * Math.Sin(angle),
                centerLon + outerRadius * lonScale * Math.Cos(angle)
            );
        }
        return ring;
    }
}
