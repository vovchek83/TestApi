namespace GeoTesterApi.Services;

/// <summary>
/// Downloads SRTM HGT tiles from the Mapzen/Tilezen public AWS bucket.
/// URL pattern: https://s3.amazonaws.com/elevation-tiles-prod/skadi/{NS}{lat:D2}/{NS}{lat:D2}{EW}{lon:D3}.hgt.gz
/// No account or API key required. Files are ~1-3 MB each (gzip-compressed).
/// Call once while online; afterwards the app works fully offline.
/// </summary>
public class SrtmDownloaderService
{
    private const string BaseUrl = "https://s3.amazonaws.com/elevation-tiles-prod/skadi";

    private readonly string _dataPath;
    private readonly HttpClient _http;
    private readonly ILogger<SrtmDownloaderService> _log;

    public SrtmDownloaderService(
        IConfiguration config,
        HttpClient http,
        ILogger<SrtmDownloaderService> log)
    {
        _dataPath = config["Srtm:DataPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "srtm");
        _http = http;
        _log = log;
        Directory.CreateDirectory(_dataPath);
    }

    /// <summary>
    /// Returns all HGT tile names required to cover the given bounding box
    /// (one tile per 1°×1° cell, plus one cell of padding on each side).
    /// </summary>
    public static IEnumerable<string> TilesForBbox(
        double minLat, double minLon, double maxLat, double maxLon)
    {
        for (int lat = (int)Math.Floor(minLat) - 1; lat <= (int)Math.Floor(maxLat) + 1; lat++)
        for (int lon = (int)Math.Floor(minLon) - 1; lon <= (int)Math.Floor(maxLon) + 1; lon++)
            yield return TileName(lat, lon);
    }

    /// <returns>List of (tileName, status) — status is "ok", "already_exists", or an error message.</returns>
    public async Task<List<(string Tile, string Status)>> DownloadAsync(
        IEnumerable<string> tileNames,
        CancellationToken ct = default)
    {
        var results = new List<(string, string)>();
        foreach (var name in tileNames.Distinct())
        {
            var dest = Path.Combine(_dataPath, name);
            if (File.Exists(dest))
            {
                results.Add((name, "already_exists"));
                continue;
            }

            try
            {
                var url = TileUrl(name);
                _log.LogInformation("Downloading {Url}", url);

                using var resp = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Tile is over ocean / no data — create an empty placeholder so we don't retry
                    await File.WriteAllBytesAsync(dest, CreateEmptySrtm3(), ct);
                    results.Add((name, "no_data_ocean"));
                    continue;
                }

                resp.EnsureSuccessStatusCode();

                await using var gz = new System.IO.Compression.GZipStream(
                    await resp.Content.ReadAsStreamAsync(ct),
                    System.IO.Compression.CompressionMode.Decompress);

                await using var fs = File.Create(dest);
                await gz.CopyToAsync(fs, ct);

                results.Add((name, "ok"));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to download {Tile}", name);
                results.Add((name, $"error: {ex.Message}"));
            }
        }
        return results;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TileName(int lat, int lon)
    {
        char ns = lat >= 0 ? 'N' : 'S';
        char ew = lon >= 0 ? 'E' : 'W';
        return $"{ns}{Math.Abs(lat):D2}{ew}{Math.Abs(lon):D3}.hgt";
    }

    private static string TileUrl(string tileName)
    {
        // Folder is NS + 2-digit lat  (e.g. N50)
        string folder = tileName[..3];
        return $"{BaseUrl}/{folder}/{tileName}.gz";
    }

    /// <summary>Creates a 1201×1201 all-zero SRTM3 placeholder for ocean tiles.</summary>
    private static byte[] CreateEmptySrtm3()
    {
        const int size = 1201 * 1201 * 2;
        return new byte[size]; // zeros = 0 m elevation (sea level)
    }
}
