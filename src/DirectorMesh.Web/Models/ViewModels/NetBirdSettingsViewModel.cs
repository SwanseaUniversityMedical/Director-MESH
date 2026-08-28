using System.ComponentModel.DataAnnotations;

namespace DirectorMesh.Web.Models.ViewModels;

public class NetBirdSettingsViewModel
{
    [Display(Name = "NetBird Server URL")]
    [Url(ErrorMessage = "Enter a valid URL, e.g. https://api.netbird.io/api")]
    public string? ApiBase { get; set; }

    [Display(Name = "API Access Token")]
    public string? ApiToken { get; set; }

    [Display(Name = "DNS Domain")]
    public string? DnsDomain { get; set; }

    // The values actually in effect once environment/appsettings fallback is applied —
    // shown read-only so operators can see what an empty field resolves to.
    public string EffectiveApiBase { get; set; } = string.Empty;
    public bool EffectiveApiTokenIsSet { get; set; }
    public string EffectiveDnsDomain { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }
}
