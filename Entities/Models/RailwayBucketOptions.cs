namespace Entities.Models;

public sealed class RailwayBucketOptions
{
    public string Bucket { get; init; } = default!;
    public string AccessKeyId { get; init; } = default!;
    public string SecretAccessKey { get; init; } = default!;
    public string Region { get; init; } = "auto";
    public string Endpoint { get; init; } = default!;
    public string ImgProxyBaseUrl { get; init; } = default!;
}
