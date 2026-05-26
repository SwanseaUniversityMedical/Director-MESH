using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DirectorMesh.SiteAgent.Services;

public class GiteaClient(HttpClient http, GiteaOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task EnsureRepoAsync(CancellationToken ct = default)
    {
        var check = await http.GetAsync(
            $"api/v1/repos/{options.EffectiveOwner}/{options.Repo}", ct);
        if (check.IsSuccessStatusCode) return;

        var resp = await http.PostAsJsonAsync("api/v1/user/repos", new
        {
            name = options.Repo,
            description = "Director-MESH site agent configurations",
            @private = false,
            auto_init = true,
            default_branch = "main"
        }, ct);
        resp.EnsureSuccessStatusCode();
    }

    public async Task CommitFileAsync(string path, string content, string message, CancellationToken ct = default)
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        var apiPath = $"api/v1/repos/{options.EffectiveOwner}/{options.Repo}/contents/{path}";

        string? sha = null;
        var existing = await http.GetAsync(apiPath, ct);
        if (existing.IsSuccessStatusCode)
        {
            var doc = await existing.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, ct);
            sha = doc.GetProperty("sha").GetString();
        }

        var body = sha != null
            ? JsonSerializer.Serialize(new { message, content = encoded, sha })
            : JsonSerializer.Serialize(new { message, content = encoded });

        var resp = await http.PutAsync(
            apiPath, new StringContent(body, Encoding.UTF8, "application/json"), ct);
        resp.EnsureSuccessStatusCode();
    }
}
