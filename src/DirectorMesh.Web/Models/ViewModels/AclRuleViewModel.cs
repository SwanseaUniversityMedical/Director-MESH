using System.ComponentModel.DataAnnotations;
using DirectorMesh.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DirectorMesh.Web.Models.ViewModels;

public class AclRuleCreateEditViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Data Plane")]
    public int DataPlaneId { get; set; }

    [Required]
    [Display(Name = "Source Service (Outbound)")]
    public int SourceServiceId { get; set; }

    [Required]
    [Display(Name = "Destination Service (Inbound)")]
    public int DestinationServiceId { get; set; }

    [Display(Name = "Action")]
    public AclAction Action { get; set; } = AclAction.Allow;

    public bool IsEnabled { get; set; } = true;

    public IEnumerable<SelectListItem> DataPlanes { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> OutboundServices { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> InboundServices { get; set; } = Enumerable.Empty<SelectListItem>();
}
