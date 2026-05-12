using GeoToolkit;
using GeoTesterApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGeoToolkit();
builder.Services.AddScoped<IPolygonService, PolygonService>();

// LOS services (singletons: elevation caches files in memory)
builder.Services.AddSingleton<IElevationService, SrtmElevationService>();
builder.Services.AddSingleton<ILosService, LosService>();

// SRTM one-time downloader (needs HttpClient)
builder.Services.AddHttpClient<SrtmDownloaderService>();

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow the Vite dev server (port 5173) to call the API during development
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Serve local map tiles from  <content-root>/wwwroot/tiles/{z}/{x}/{y}.png
// Users download tiles (e.g. with tiledl / Maperitive / MOBAC) and place them there.
// Fallback: map will show a grey background when tiles are absent.
app.UseStaticFiles();

app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
