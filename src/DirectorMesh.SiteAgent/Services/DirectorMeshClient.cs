using System.Net.Http.Json;
using System.Text.Json;

namespace DirectorMesh.SiteAgent.Services;

public class DirectorMeshClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient http;
    private readonly DirectorMeshOptions options;

    // Connection details are applied per instance (rather than when the HttpClient is
    // registered) so that changes made on the Settings screen take effect immediately.
    public DirectorMeshClient(HttpClient http, DirectorMeshOptions options)
    {
        this.http = http;
        this.options = options;
        if (options.IsConfigured)
        {
            http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            http.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
        }
    }

    public async Task<List<DataPlaneConfigResponse>> GetConfigsAsync(CancellationToken ct = default)
    {
        var result = await http.GetFromJsonAsync<List<DataPlaneConfigResponse>>(
            $"api/site-agent/{options.SiteId}/configs", JsonOptions, ct);
        return result ?? [];
    }

    public async Task<ApprovalResult?> ApproveAsync(int configId, string? notes = null, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/site-agent/{options.SiteId}/configs/{configId}/approve",
            new { notes }, ct);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApprovalResult>(JsonOptions, ct);
    }

    public async Task<bool> RejectAsync(int configId, string reason, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync(
            $"api/site-agent/{options.SiteId}/configs/{configId}/reject",
            new { reason }, ct);
        return response.IsSuccessStatusCode;
    }
}

public record DataPlaneConfigResponse(
    int DataPlaneId,
    string DataPlane,
    ConfigSummary? Pending,
    ConfigSummary? LastApplied,
    ConfigSummary? LastDecision);

public record ConfigSummary(
    int Id,
    int Version,
    string Status,
    string? ConfigurationJson,
    string? K8sManifestYaml,
    DateTime CreatedAt,
    DateTime? SentAt,
    DateTime? ApprovedAt,
    DateTime? AppliedAt,
    string? RejectionReason);

public record ApprovalResult(string SiteName, string DataPlaneName, int Version, string? K8sManifestYaml);
