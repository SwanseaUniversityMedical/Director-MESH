namespace DirectorMesh.Web.Models.Domain;

// Singleton row (always Id = 1) holding operator-supplied overrides for the NetBird
// integration. Any field left blank here falls back to the NetBird:* configuration
// (appsettings.json / NetBird__* environment variables) — see NetBirdSettingsResolver.
public class NetBirdSettings
{
    public int Id { get; set; }
    public string? ApiBase { get; set; }
    public string? ApiToken { get; set; }
    public string? DnsDomain { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
