using Entities.Models;
using Microsoft.AspNetCore.Http;

namespace Services.Contracts;

public interface IStorageService
{
    Task<(string key, string publicUrl)> UploadAsync(
        IFormFile file,
        string keyPrefix,
        CancellationToken ct = default
    );

    Task DeleteAsync(string key, CancellationToken ct = default);
}

