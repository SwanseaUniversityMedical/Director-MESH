namespace DirectorMesh.Web.Services;

public record NetBirdEffectiveSettings(string ApiBase, string ApiToken, string DnsDomain);

public interface INetBirdSettingsResolver
{
    // Merges the operator-supplied DB row (Settings > NetBird) with the NetBird:*
    // configuration (appsettings.json / NetBird__* environment variables). A blank
    // field in the database falls back to the configured value.
    Task<NetBirdEffectiveSettings> ResolveAsync(CancellationToken ct = default);
}
