using GeoJSON.Net.Feature;

namespace GeoTesterApi.Services;

public interface IPolygonService
{
    FeatureCollection CreatePolygon();
    FeatureCollection CreateMultiPolygon();
    FeatureCollection CreateDonutPolygon();
    FeatureCollection CreateMultiPolygonFromWkt();
    FeatureCollection CreateUnionPolygon();
}
