using System.Collections.Concurrent;

namespace GeoTesterApi.Services;

/// <summary>
/// Reads SRTM .hgt files (SRTM1 or SRTM3) from a local directory and returns
/// bilinearly-interpolated terrain elevation at any lat/lon.
///
/// File naming: N32E034.hgt  (1°×1° tile, lat 32–33 N, lon 34–35 E)
/// SRTM3 = 1201×1201 INT16 big-endian  (~90 m resolution)
/// SRTM1 = 3601×3601 INT16 big-endian  (~30 m resolution)
///
/// Download tiles from: https://dwtkns.com/srtm30m/ or https://srtm.csi.cgiar.org/
/// Place them in the directory configured by Srtm:DataPath (default: ./srtm)
/// </summary>
public class SrtmElevationService : IElevationService
{
    private readonly string _dataPath;
    private readonly ConcurrentDictionary<string, short[]> _cache = new();

    public SrtmElevationService(IConfiguration config)
    {
        _dataPath = config["Srtm:DataPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "srtm");
        Directory.CreateDirectory(_dataPath);
    }

    public double GetElevation(double lat, double lon)
    {
        int latFloor = (int)Math.Floor(lat);
        int lonFloor = (int)Math.Floor(lon);

        var fileName = BuildFileName(latFloor, lonFloor);
        var data = _cache.GetOrAdd(fileName, LoadHgtFile);
        if (data.Length == 0) return 0.0;

        // SRTM3 = 1201, SRTM1 = 3601
        int size = data.Length == 1201 * 1201 ? 1201 : 3601;

        // Row 0 = northernmost latitude (latFloor + 1), col 0 = westernmost (lonFloor)
        double rowFrac = (latFloor + 1.0 - lat) * (size - 1);
        double colFrac = (lon - lonFloor) * (size - 1);

        int row = (int)rowFrac;
        int col = (int)colFrac;
        double dr = rowFrac - row;
        double dc = colFrac - col;

        row = Math.Clamp(row, 0, size - 2);
        col = Math.Clamp(col, 0, size - 2);

        double v00 = FixVoid(data[row * size + col]);
        double v01 = FixVoid(data[row * size + col + 1]);
        double v10 = FixVoid(data[(row + 1) * size + col]);
        double v11 = FixVoid(data[(row + 1) * size + col + 1]);

        return v00 * (1 - dr) * (1 - dc)
             + v01 * (1 - dr) * dc
             + v10 * dr * (1 - dc)
             + v11 * dr * dc;
    }

    private short[] LoadHgtFile(string fileName)
    {
        var path = Path.Combine(_dataPath, fileName);
        if (!File.Exists(path)) return [];

        var bytes = File.ReadAllBytes(path);
        int count = bytes.Length / 2;
        var data = new short[count];
        for (int i = 0; i < count; i++)
            // Big-endian INT16 → host INT16
            data[i] = (short)((bytes[i * 2] << 8) | (bytes[i * 2 + 1] & 0xFF));

        return data;
    }

    private static string BuildFileName(int lat, int lon)
    {
        char ns = lat >= 0 ? 'N' : 'S';
        char ew = lon >= 0 ? 'E' : 'W';
        return $"{ns}{Math.Abs(lat):D2}{ew}{Math.Abs(lon):D3}.hgt";
    }

    // SRTM void value is -32768; treat as sea level
    private static double FixVoid(short v) => v == -32768 ? 0.0 : v;
}
