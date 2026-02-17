using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Services.Contracts;

namespace Services;

public sealed class DiskStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;

    public DiskStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(string key, string publicUrl)> UploadAsync(IFormFile file, string keyPrefix, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Boş dosya yüklenemez.");

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folder = Path.Combine(webRoot, keyPrefix.Replace("/", Path.DirectorySeparatorChar.ToString()));
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        var key = $"{keyPrefix.TrimEnd('/')}/{fileName}";
        var publicUrl = $"/{keyPrefix.Trim('/')}/{fileName}";
        return (key, publicUrl);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var rel = key.Replace("/", Path.DirectorySeparatorChar.ToString());
        var fullPath = Path.Combine(webRoot, rel);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
