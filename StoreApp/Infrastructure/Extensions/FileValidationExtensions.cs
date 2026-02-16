using Microsoft.AspNetCore.Http;

namespace StoreApp.Infrastructure.Extensions;

public static class FileValidationExtensions
{
    private static readonly string[] AllowedExtensions =
        { ".jpg", ".jpeg", ".png", ".webp" };

    public static bool IsValidImage(this IFormFile file, int maxMb, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (file == null || file.Length == 0)
        {
            errorMessage = "Dosya seçilmedi.";
            return false;
        }

        long maxBytes = maxMb * 1024L * 1024L;
        if (file.Length > maxBytes)
        {
            errorMessage = $"Dosya boyutu en fazla {maxMb} MB olabilir.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
        {
            errorMessage = "Sadece JPG, JPEG, PNG veya WEBP formatı yüklenebilir.";
            return false;
        }

        return true;
    }
}
