namespace DirectorMesh.SiteAgent.Services;

public class DirectorMeshOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int SiteId { get; set; }
    public string ApiKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        Uri.TryCreate(BaseUrl, UriKind.Absolute, out _) && SiteId > 0 && !string.IsNullOrWhiteSpace(ApiKey);
}
