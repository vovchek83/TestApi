using GeoTesterApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GeoTesterApi.Controllers;

public record DownloadRequest(
    double MinLat, double MinLon,
    double MaxLat, double MaxLon);

public record DownloadResult(string Tile, string Status);

[ApiController]
[Route("[controller]")]
public class SrtmController : ControllerBase
{
    private readonly SrtmDownloaderService _downloader;

    public SrtmController(SrtmDownloaderService downloader) => _downloader = downloader;

    /// <summary>
    /// Lists which SRTM tiles are already downloaded vs missing for the given bbox.
    /// </summary>
    [HttpGet("status")]
    public IActionResult Status(
        [FromQuery] double minLat, [FromQuery] double minLon,
        [FromQuery] double maxLat, [FromQuery] double maxLon,
        [FromServices] IConfiguration config)
    {
        var dataPath = config["Srtm:DataPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "srtm");

        var tiles = SrtmDownloaderService.TilesForBbox(minLat, minLon, maxLat, maxLon)
            .Select(t => new
            {
                tile = t,
                exists = System.IO.File.Exists(Path.Combine(dataPath, t)),
            });

        return Ok(tiles);
    }

    /// <summary>
    /// Downloads missing SRTM tiles for the given bounding box from the public AWS bucket.
    /// Run once while online; the app works offline afterwards.
    /// </summary>
    [HttpPost("download")]
    public async Task<IActionResult> Download(
        [FromBody] DownloadRequest req,
        CancellationToken ct)
    {
        var tiles = SrtmDownloaderService.TilesForBbox(
            req.MinLat, req.MinLon, req.MaxLat, req.MaxLon);

        var results = await _downloader.DownloadAsync(tiles, ct);
        return Ok(results.Select(r => new DownloadResult(r.Tile, r.Status)));
    }
}
