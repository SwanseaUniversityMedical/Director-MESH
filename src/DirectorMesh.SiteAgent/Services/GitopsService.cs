using Microsoft.Extensions.Logging;

namespace DirectorMesh.SiteAgent.Services;

public class GitopsService(GiteaClient gitea, GiteaOptions options, ILogger<GitopsService> logger)
{
    public async Task PushApprovedConfigAsync(ApprovalResult config, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(options.BaseUrl) || string.IsNullOrEmpty(options.Password))
        {
            logger.LogWarning("Gitea not configured — skipping GitOps push.");
            return;
        }

        await gitea.EnsureRepoAsync(ct);

        var siteSlug = Slug(config.SiteName);
        var dpSlug = Slug(config.DataPlaneName);
        var repoUrl = options.EffectiveRepoUrl;
        var commitMsg = $"Director-MESH: {config.SiteName}/{config.DataPlaneName} v{config.Version}";

        logger.LogInformation("Pushing config {Site}/{DP} v{Version} to Gitea",
            config.SiteName, config.DataPlaneName, config.Version);

        await gitea.CommitFileAsync(
            $"manifests/{siteSlug}/{dpSlug}/k8s-manifest.yaml",
            config.K8sManifestYaml ?? string.Empty,
            commitMsg, ct);

        var appYaml = ArgoManifestBuilder.BuildDataPlaneApp(
            config.SiteName, config.DataPlaneName,
            repoUrl, $"manifests/{siteSlug}/{dpSlug}");
        await gitea.CommitFileAsync(
            $"apps/{siteSlug}/{dpSlug}-app.yaml",
            appYaml, commitMsg, ct);

        var appOfAppsYaml = ArgoManifestBuilder.BuildAppOfApps(
            config.SiteName, repoUrl, $"apps/{siteSlug}");
        await gitea.CommitFileAsync(
            $"apps/{siteSlug}/app-of-apps.yaml",
            appOfAppsYaml, commitMsg, ct);
    }

    private static string Slug(string name) =>
        new string(name.ToLower().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');
}
