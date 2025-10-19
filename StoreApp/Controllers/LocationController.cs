using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public LocationsController(IWebHostEnvironment env) => _env = env;

    private string DataPath(params string[] parts) =>
        Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "data", "tr", Path.Combine(parts));

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var path = DataPath("sehirler.json");
        if (!System.IO.File.Exists(path)) return NotFound("sehirler.json yok");
        var text = await System.IO.File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<List<City>>(text, _json) ?? new();
        // Ada göre sıralı gönderelim
        return Ok(data.OrderBy(c => c.Name));
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] string cityId)
    {
        if (string.IsNullOrWhiteSpace(cityId)) return BadRequest("cityId gerekli");
        var path = DataPath("ilceler.json");
        if (!System.IO.File.Exists(path)) return NotFound("ilceler.json yok");
        var text = await System.IO.File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<List<District>>(text, _json) ?? new();
        return Ok(data.Where(d => d.CityId == cityId).OrderBy(d => d.Name));
    }

    [HttpGet("neighborhoods")]
    public async Task<IActionResult> GetNeighborhoods([FromQuery] string cityId, [FromQuery] string districtId)
    {
        if (string.IsNullOrWhiteSpace(cityId) || string.IsNullOrWhiteSpace(districtId))
            return BadRequest("cityId ve districtId gerekli");

        // Mahalleler 4 dosyaya bölünmüş: hepsini birleştir.
        var files = new[] {
            DataPath("mahalleler-1.json"),
            DataPath("mahalleler-2.json"),
            DataPath("mahalleler-3.json"),
            DataPath("mahalleler-4.json")
        };

        var list = new List<Neighborhood>();
        foreach (var f in files)
        {
            if (!System.IO.File.Exists(f)) continue;
            var text = await System.IO.File.ReadAllTextAsync(f);
            var chunk = JsonSerializer.Deserialize<List<Neighborhood>>(text, _json);
            if (chunk is { Count: > 0 }) list.AddRange(chunk);
        }

        var result = list
            .Where(m => m.CityId == cityId && m.DistrictId == districtId)
            .OrderBy(m => m.Name);

        return Ok(result);
    }
}
