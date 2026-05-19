namespace DirectorMesh.Web.Models.Domain;

public class Site
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public SiteStatus Status { get; set; } = SiteStatus.Provisioning;
    public string? ApiKey { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<DataPlane> DataPlanes { get; set; } = new List<DataPlane>();
    public ICollection<NetworkService> Services { get; set; } = new List<NetworkService>();
    public ICollection<SiteConfiguration> Configurations { get; set; } = new List<SiteConfiguration>();
}

public enum SiteStatus { Provisioning, Active, Inactive, Error }
