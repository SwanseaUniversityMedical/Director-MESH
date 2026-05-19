using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DirectorMesh.Web.Services;

public class NetBirdService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<NetBirdService> logger) : INetBirdService
{
    private readonly string _apiBase = configuration["NetBird:ApiBase"] ?? "https://api.netbird.io/api";
    private readonly string _apiToken = configuration["NetBird:ApiToken"] ?? string.Empty;

    public async Task<string?> CreatePeerGroupAsync(string groupName, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);
            var body = JsonSerializer.Serialize(new { name = groupName });
            var response = await client.PostAsync($"{_apiBase}/groups",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("id").GetString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create NetBird peer group {Name}", groupName);
            return null;
        }
    }

    public async Task<bool> CreatePolicyAsync(string name, string sourceGroupId, string destinationGroupId, string protocol, int port, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);
            var body = JsonSerializer.Serialize(new
            {
                name,
                enabled = true,
                rules = new[]
                {
                    new
                    {
                        name,
                        enabled = true,
                        action = "accept",
                        bidirectional = false,
                        protocol,
                        ports = new[] { port.ToString() },
                        sources = new[] { sourceGroupId },
                        destinations = new[] { destinationGroupId }
                    }
                }
            });
            var response = await client.PostAsync($"{_apiBase}/policies",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create NetBird policy {Name}", name);
            return false;
        }
    }

    public async Task<bool> DeletePolicyAsync(string policyId, CancellationToken ct = default)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _apiToken);
            var response = await client.DeleteAsync($"{_apiBase}/policies/{policyId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete NetBird policy {Id}", policyId);
            return false;
        }
    }
}
