using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public LocationsController(IWebHostEnvironment env) => _env = env;

    // Hem wwwroot/data/tr hem de contentRoot/data/tr altında deneyen yardımcılar
    private string PathInWebRoot(params string[] parts)
        => Path.Combine(_env.WebRootPath ?? string.Empty, "data", "tr", Path.Combine(parts));

    private string PathInContentRoot(params string[] parts)
        => Path.Combine(_env.ContentRootPath ?? string.Empty, "data", "tr", Path.Combine(parts));

    private string? FindExistingPath(params string[] parts)
    {
        var p1 = PathInWebRoot(parts);
        if (System.IO.File.Exists(p1)) return p1;
        var p2 = PathInContentRoot(parts);
        if (System.IO.File.Exists(p2)) return p2;
        return null;
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var path = FindExistingPath("sehirler.json");
        if (path is null) return NotFound("sehirler.json bulunamadı (wwwroot/data/tr veya data/tr)");
        var text = await System.IO.File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<List<City>>(text, _json) ?? new();
        return Ok(data.OrderBy(c => c.Name));
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] string cityId)
    {
        if (string.IsNullOrWhiteSpace(cityId)) return BadRequest("cityId gerekli");
        var path = FindExistingPath("ilceler.json");
        if (path is null) return NotFound("ilceler.json bulunamadı");
        var text = await System.IO.File.ReadAllTextAsync(path);
        var data = JsonSerializer.Deserialize<List<District>>(text, _json) ?? new();
        return Ok(data.Where(d => d.CityId == cityId).OrderBy(d => d.Name));
    }

    [HttpGet("neighborhoods")]
    public async Task<IActionResult> GetNeighborhoods([FromQuery] string cityId, [FromQuery] string districtId)
    {
        if (string.IsNullOrWhiteSpace(cityId) || string.IsNullOrWhiteSpace(districtId))
            return BadRequest("cityId ve districtId gerekli");

        var files = new[]
        {
            FindExistingPath("mahalleler-1.json"),
            FindExistingPath("mahalleler-2.json"),
            FindExistingPath("mahalleler-3.json"),
            FindExistingPath("mahalleler-4.json")
        }.Where(p => p is not null)!;

        var list = new List<Neighborhood>();
        foreach (var f in files)
        {
            var text = await System.IO.File.ReadAllTextAsync(f!);
            var chunk = JsonSerializer.Deserialize<List<Neighborhood>>(text, _json);
            if (chunk?.Count > 0) list.AddRange(chunk);
        }

        var result = list
            .Where(m => m.CityId == cityId && m.DistrictId == districtId)
            .OrderBy(m => m.Name);

        return Ok(result);
    }
}