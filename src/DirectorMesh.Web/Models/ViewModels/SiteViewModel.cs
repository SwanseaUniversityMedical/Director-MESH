using System.ComponentModel.DataAnnotations;
using DirectorMesh.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DirectorMesh.Web.Models.ViewModels;

public class SiteCreateEditViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Data Planes")]
    public List<int> DataPlaneIds { get; set; } = new();

    public SiteStatus Status { get; set; } = SiteStatus.Provisioning;

    public IEnumerable<SelectListItem> DataPlanes { get; set; } = Enumerable.Empty<SelectListItem>();
}
