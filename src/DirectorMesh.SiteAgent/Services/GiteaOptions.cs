namespace DirectorMesh.SiteAgent.Services;

public class GiteaOptions
{
    public string BaseUrl { get; set; } = "http://gitea:3000";
    public string Username { get; set; } = "gitea";
    public string Password { get; set; } = string.Empty;
    public string Repo { get; set; } = "director-mesh-configs";

    // Owner defaults to Username if not set
    public string? Owner { get; set; }

    // URL ArgoCD uses to clone — defaults to BaseUrl/{owner}/{repo}.git
    // Override when ArgoCD reaches Gitea via a different hostname (e.g. external DNS)
    public string? RepoUrl { get; set; }

    public string EffectiveOwner => string.IsNullOrEmpty(Owner) ? Username : Owner;
    public string EffectiveRepoUrl =>
        RepoUrl ?? $"{BaseUrl.TrimEnd('/')}/{EffectiveOwner}/{Repo}.git";
}
