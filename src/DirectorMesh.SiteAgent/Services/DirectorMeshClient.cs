using System.Net.Http.Json;
using System.Text.Json;

namespace DirectorMesh.SiteAgent.Services;

public class DirectorMeshClient(HttpClient http, DirectorMeshOptions options)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
