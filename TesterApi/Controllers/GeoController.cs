using GeoJSON.Net.Feature;
using GeoTesterApi.Services;
using GeoToolkit.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GeoTesterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GeoController : ControllerBase
    {
        private readonly IGeoUnionService _geoUnionService;
        private readonly IPolygonService _polygonService;

        public GeoController(IGeoUnionService geoUnionService, IPolygonService polygonService)
        {
            _geoUnionService = geoUnionService;
            _polygonService = polygonService;
        }

        [HttpGet("polygon", Name = "GetPolygon")]
        public ActionResult<FeatureCollection> GetPolygon() =>
            Ok(_polygonService.CreatePolygon());

        [HttpGet("multipolygon", Name = "GetMultiPolygon")]
        public ActionResult<FeatureCollection> GetMultiPolygon() =>
            Ok(_geoUnionService.Union(_polygonService.CreateMultiPolygon()));

        [HttpGet("donut", Name = "GetDonutPolygon")]
        public ActionResult<FeatureCollection> GetDonutPolygon() =>
            Ok(_polygonService.CreateDonutPolygon());

        [HttpGet("wkt-multipolygon", Name = "GetWktMultiPolygon")]
        public ActionResult<FeatureCollection> GetWktMultiPolygon() =>
            Ok(_polygonService.CreateMultiPolygonFromWkt());

        [HttpGet("union-polygon", Name = "GetUnionPolygon")]
        public ActionResult<FeatureCollection> GetUnionPolygon() =>
            Ok(_polygonService.CreateUnionPolygon());

        [HttpPost("union", Name = "UnionPolygons")]
        public ActionResult<FeatureCollection> UnionPolygons([FromBody] FeatureCollection featureCollection) =>
            Ok(_geoUnionService.Union(featureCollection));
    }
}
