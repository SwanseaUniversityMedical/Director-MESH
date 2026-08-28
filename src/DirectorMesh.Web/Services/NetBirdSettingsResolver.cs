using DirectorMesh.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Services;

public class NetBirdSettingsResolver(ApplicationDbContext db, IConfiguration configuration) : INetBirdSettingsResolver
{
    public async Task<NetBirdEffectiveSettings> ResolveAsync(CancellationToken ct = default)
    {
        var stored = await db.NetBirdSettings.FirstOrDefaultAsync(ct);
        return new NetBirdEffectiveSettings(
            FirstNonEmpty(stored?.ApiBase, configuration["NetBird:ApiBase"]) ?? "https://api.netbird.io/api",
            FirstNonEmpty(stored?.ApiToken, configuration["NetBird:ApiToken"]) ?? string.Empty,
            FirstNonEmpty(stored?.DnsDomain, configuration["NetBird:DnsDomain"]) ?? string.Empty);
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
