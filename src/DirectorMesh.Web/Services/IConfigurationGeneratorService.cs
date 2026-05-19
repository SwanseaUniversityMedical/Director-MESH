using DirectorMesh.Web.Models.Domain;

namespace DirectorMesh.Web.Services;

public interface IConfigurationGeneratorService
{
    Task<SiteConfiguration> GenerateAsync(Site site, DataPlane dataPlane, IEnumerable<AclRule> dataPlaneRules, CancellationToken ct = default);
}
