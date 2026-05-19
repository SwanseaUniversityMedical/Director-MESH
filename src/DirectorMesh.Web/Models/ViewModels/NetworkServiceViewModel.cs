using System.ComponentModel.DataAnnotations;
using DirectorMesh.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DirectorMesh.Web.Models.ViewModels;

public class NetworkServiceCreateEditViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Site")]
    public int SiteId { get; set; }

    [Required]
    [Display(Name = "Direction")]
    public ServiceDirection Direction { get; set; }

    [Required]
    [Display(Name = "Protocol")]
    public ServiceProtocol Protocol { get; set; } = ServiceProtocol.TCP;

    [Required, Range(1, 65535)]
    [Display(Name = "Port")]
    public int Port { get; set; }

    // NetBird peer identity for this service
    [MaxLength(200)]
    [Display(Name = "NetBird Peer Name")]
    public string? NetBirdPeerName { get; set; }

    [MaxLength(200)]
    [Display(Name = "NetBird DNS Name")]
    public string? NetBirdDnsName { get; set; }

    [MaxLength(45)]
    [Display(Name = "NetBird IP Address")]
    public string? NetBirdIpAddress { get; set; }

    [MaxLength(200)]
    [Display(Name = "NetBird Setup Key")]
    public string? NetBirdSetupKey { get; set; }

    // Inbound: where this peer proxies traffic to inside the cluster
    [MaxLength(500)]
    [Display(Name = "Local Endpoint")]
    public string? LocalEndpoint { get; set; }

    // Outbound: NetBird DNS name of the destination inbound service peer
    [MaxLength(500)]
    [Display(Name = "Remote NetBird DNS Name")]
    public string? RemoteNetBirdDnsName { get; set; }

    public bool IsEnabled { get; set; } = true;

    public IEnumerable<SelectListItem> Sites { get; set; } = Enumerable.Empty<SelectListItem>();
}
