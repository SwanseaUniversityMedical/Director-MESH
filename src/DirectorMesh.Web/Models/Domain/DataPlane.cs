namespace DirectorMesh.Web.Models.Domain;

public class DataPlane
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? NetBirdNetworkId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Site> Sites { get; set; } = new List<Site>();
    public ICollection<AclRule> AclRules { get; set; } = new List<AclRule>();
    public ICollection<NetworkService> Services { get; set; } = new List<NetworkService>();
}
