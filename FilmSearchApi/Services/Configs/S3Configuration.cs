namespace FilmSearchApi.Services.Configs;

public class S3Configuration
{
    public string ServiceUrl { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string AuthenticationRegion { get; set; } = null!;
}