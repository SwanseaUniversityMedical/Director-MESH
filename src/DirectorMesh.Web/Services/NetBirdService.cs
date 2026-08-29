using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DirectorMesh.Web.Services;

public class NetBirdService(IHttpClientFactory httpClientFactory, INetBirdSettingsResolver settingsResolver, ILogger<NetBirdService> logger) : INetBirdService
{
    public async Task<string?> CreatePeerGroupAsync(string groupName, CancellationToken ct = default)
    {
        try
        {
            var settings = await settingsResolver.ResolveAsync(ct);
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.ApiToken);
            var body = JsonSerializer.Serialize(new { name = groupName });
            var response = await client.PostAsync($"{settings.ApiBase}/groups",
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
            var settings = await settingsResolver.ResolveAsync(ct);
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.ApiToken);
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
            var response = await client.PostAsync($"{settings.ApiBase}/policies",
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
            var settings = await settingsResolver.ResolveAsync(ct);
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.ApiToken);
            var response = await client.DeleteAsync($"{settings.ApiBase}/policies/{policyId}", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete NetBird policy {Id}", policyId);
            return false;
        }
    }

    public async Task<string?> CreateSetupKeyAsync(string name, CancellationToken ct = default)
    {
        try
        {
            var settings = await settingsResolver.ResolveAsync(ct);
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", settings.ApiToken);
            var body = JsonSerializer.Serialize(new
            {
                name,
                type = "reusable",
                expires_in = 31536000,
                revoked = false,
                usage_limit = 0
            });
            var response = await client.PostAsync($"{settings.ApiBase}/setup-keys",
                new StringContent(body, Encoding.UTF8, "application/json"), ct);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("key").GetString();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create NetBird setup key {Name}", name);
            return null;
        }
    }
}
