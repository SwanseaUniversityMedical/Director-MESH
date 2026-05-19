namespace DirectorMesh.Web.Models.Domain;

public class AclRule
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int SourceServiceId { get; set; }
    public NetworkService SourceService { get; set; } = null!;
    public int DestinationServiceId { get; set; }
    public NetworkService DestinationService { get; set; } = null!;
    public AclAction Action { get; set; } = AclAction.Allow;
    public bool IsEnabled { get; set; } = true;
    public int DataPlaneId { get; set; }
    public DataPlane DataPlane { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum AclAction { Allow, Deny }
