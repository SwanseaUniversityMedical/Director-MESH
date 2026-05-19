using DirectorMesh.Web.Models.Domain;

namespace DirectorMesh.Web.Services;

public interface IConfigurationGeneratorService
{
    Task<SiteConfiguration> GenerateAsync(Site site, IEnumerable<AclRule> applicableRules, CancellationToken ct = default);
}
