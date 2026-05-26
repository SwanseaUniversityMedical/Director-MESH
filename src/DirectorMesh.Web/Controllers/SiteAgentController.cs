using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[ApiController]
[Route("api/site-agent")]
public class SiteAgentController(ApplicationDbContext db) : ControllerBase
{
    private async Task<Site?> AuthenticateSite(int siteId)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey)) return null;
        var site = await db.Sites.Include(s => s.DataPlanes).FirstOrDefaultAsync(s => s.Id == siteId);
        if (site == null || site.ApiKey != apiKey) return null;
        return site;
    }

    [HttpGet("{siteId}/configs")]
    public async Task<IActionResult> GetConfigs(int siteId)
    {
        var site = await AuthenticateSite(siteId);
        if (site == null) return Unauthorized();

        var result = new List<DataPlaneConfigResponse>();
        foreach (var dp in site.DataPlanes.OrderBy(dp => dp.Name))
        {
            var pending = await db.SiteConfigurations
                .Where(c => c.SiteId == siteId && c.DataPlaneId == dp.Id && c.Status == ConfigurationStatus.Sent)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            var lastApplied = await db.SiteConfigurations
                .Where(c => c.SiteId == siteId && c.DataPlaneId == dp.Id && c.Status == ConfigurationStatus.Applied)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            var lastDecision = await db.SiteConfigurations
                .Where(c => c.SiteId == siteId && c.DataPlaneId == dp.Id &&
                            (c.Status == ConfigurationStatus.Approved || c.Status == ConfigurationStatus.Rejected))
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            result.Add(new DataPlaneConfigResponse(
                dp.Id,
                dp.Name,
                pending == null ? null : ToSummary(pending),
                lastApplied == null ? null : ToSummary(lastApplied),
                lastDecision == null ? null : ToSummary(lastDecision)
            ));
        }

        return Ok(result);
    }

    [HttpPost("{siteId}/configs/{configId}/approve")]
    public async Task<IActionResult> Approve(int siteId, int configId, [FromBody] ApproveRequest? body)
    {
        var site = await AuthenticateSite(siteId);
        if (site == null) return Unauthorized();

        var config = await db.SiteConfigurations
            .Include(c => c.Site)
            .Include(c => c.DataPlane)
            .FirstOrDefaultAsync(c => c.Id == configId && c.SiteId == siteId);
        if (config == null) return NotFound();
        if (config.Status != ConfigurationStatus.Sent)
            return BadRequest(new { error = "Config is not in Sent status." });

        config.Status = ConfigurationStatus.Approved;
        config.ApprovedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(body?.Notes))
            config.Notes = body.Notes;
        await db.SaveChangesAsync();

        return Ok(new ApprovalResult(
            config.Site.Name,
            config.DataPlane?.Name ?? "default",
            config.Version,
            config.K8sManifestYaml));
    }

    [HttpPost("{siteId}/configs/{configId}/reject")]
    public async Task<IActionResult> Reject(int siteId, int configId, [FromBody] RejectRequest body)
    {
        var site = await AuthenticateSite(siteId);
        if (site == null) return Unauthorized();

        var config = await db.SiteConfigurations.FirstOrDefaultAsync(c => c.Id == configId && c.SiteId == siteId);
        if (config == null) return NotFound();
        if (config.Status != ConfigurationStatus.Sent)
            return BadRequest(new { error = "Config is not in Sent status." });

        config.Status = ConfigurationStatus.Rejected;
        config.RejectionReason = body.Reason;
        await db.SaveChangesAsync();

        return Ok(new { message = "Configuration rejected." });
    }

    private static ConfigSummary ToSummary(SiteConfiguration c) =>
        new(c.Id, c.Version, c.Status.ToString(), c.ConfigurationJson, c.K8sManifestYaml,
            c.CreatedAt, c.SentAt, c.ApprovedAt, c.AppliedAt, c.RejectionReason);
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

public record ApproveRequest(string? Notes);
public record RejectRequest(string Reason);
public record ApprovalResult(string SiteName, string DataPlaneName, int Version, string? K8sManifestYaml);
