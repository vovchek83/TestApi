using GeoTesterApi.Models;
using GeoTesterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoTesterApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LosController : ControllerBase
{
    private readonly ILosService _los;

    public LosController(ILosService los) => _los = los;

    /// <summary>Calculate Line-of-Sight profile between TX and RX.</summary>
    [HttpPost("calculate", Name = "CalculateLos")]
    public ActionResult<LosResponse> Calculate([FromBody] LosRequest request)
        => Ok(_los.Calculate(request));

    /// <summary>
    /// Calculate a viewshed polygon from TX — the area visible in all directions
    /// up to maxRangeKm. Returns a GeoJSON-ready polygon ring.
    /// </summary>
    [HttpPost("viewshed", Name = "CalculateViewshed")]
    public ActionResult<ViewshedResponse> Viewshed([FromBody] ViewshedRequest request)
        => Ok(_los.CalculateViewshed(request));
}
