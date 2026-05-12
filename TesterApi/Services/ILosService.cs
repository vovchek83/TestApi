using GeoTesterApi.Models;

namespace GeoTesterApi.Services;

public interface ILosService
{
    LosResponse Calculate(LosRequest request);
    ViewshedResponse CalculateViewshed(ViewshedRequest request);
}
