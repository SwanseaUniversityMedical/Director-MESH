namespace DirectorMesh.Web.Models.Domain;

public class SiteConfiguration
{
    public int Id { get; set; }
    public int SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public int? DataPlaneId { get; set; }
    public DataPlane? DataPlane { get; set; }
    public int Version { get; set; }
    public ConfigurationStatus Status { get; set; } = ConfigurationStatus.Draft;
    public string? ContentHash { get; set; }
    public string? ConfigurationJson { get; set; }
    public string? HaProxyConfig { get; set; }
    public string? K8sManifestYaml { get; set; }
    public string? ApprovalToken { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? AppliedAt { get; set; }
}

public enum ConfigurationStatus { Draft, Sent, Approved, Rejected, Applied }
