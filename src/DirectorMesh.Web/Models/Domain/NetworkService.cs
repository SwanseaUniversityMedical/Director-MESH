namespace DirectorMesh.Web.Models.Domain;

public class NetworkService
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int SiteId { get; set; }
    public Site Site { get; set; } = null!;
    public ServiceDirection Direction { get; set; }
    public ServiceProtocol Protocol { get; set; } = ServiceProtocol.TCP;
    public int Port { get; set; }

    // NetBird peer — each service registers as its own peer in NetBird
    public string? NetBirdPeerName { get; set; }
    public string? NetBirdPeerId { get; set; }
    public string? NetBirdDnsName { get; set; }
    public string? NetBirdIpAddress { get; set; }
    public string? NetBirdGroupId { get; set; }
    public string? NetBirdSetupKey { get; set; }

    // Inbound: the local K8s service address this peer proxies to (e.g. "postgres-svc:5432")
    public string? LocalEndpoint { get; set; }
    // Outbound: the NetBird DNS name of the destination inbound service peer
    public string? RemoteNetBirdDnsName { get; set; }

    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<AclRule> SourceAclRules { get; set; } = new List<AclRule>();
    public ICollection<AclRule> DestinationAclRules { get; set; } = new List<AclRule>();
}

public enum ServiceDirection { Inbound, Outbound }
public enum ServiceProtocol { TCP, UDP }
