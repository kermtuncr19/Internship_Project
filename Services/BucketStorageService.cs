using Microsoft.AspNetCore.Http;

using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.StaticFiles;
using Entities.Models;
using Services.Contracts;

namespace Services;

public sealed class BucketStorageService : IStorageService
{
    private readonly RailwayBucketOptions _opt;
    private readonly IAmazonS3 _s3;
    private readonly FileExtensionContentTypeProvider _ct = new();

    public BucketStorageService(RailwayBucketOptions opt)
    {
        _opt = opt;

        var creds = new BasicAWSCredentials(_opt.AccessKeyId, _opt.SecretAccessKey);

        var cfg = new AmazonS3Config
        {
            ServiceURL = _opt.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = _opt.Region
        };

        _s3 = new AmazonS3Client(creds, cfg);
    }

    public async Task<(string key, string publicUrl)> UploadAsync(
        IFormFile file,
        string keyPrefix,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Boş dosya yüklenemez.");

        var ext = Path.GetExtension(file.FileName);
        var key = $"{keyPrefix.TrimEnd('/')}/{Guid.NewGuid():N}{ext}";

        if (!_ct.TryGetContentType(file.FileName, out var contentType))
            contentType = "application/octet-stream";

        await using var stream = file.OpenReadStream();

        var req = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(req, ct);

        var baseUrl = _opt.ImgProxyBaseUrl.TrimEnd('/');
        var publicUrl = $"{baseUrl}/unsafe/plain/s3://{_opt.Bucket}/{key}";

        return (key, publicUrl);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var req = new DeleteObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key
        };

        await _s3.DeleteObjectAsync(req, ct);
    }
}
