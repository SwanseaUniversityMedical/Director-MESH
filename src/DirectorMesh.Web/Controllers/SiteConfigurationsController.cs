using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using DirectorMesh.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class SiteConfigurationsController(ApplicationDbContext db, IConfigurationGeneratorService generator) : Controller
{
    private static string SanitizeName(string name) =>
        new string(name.ToLower().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray()).Trim('-');

    public async Task<IActionResult> Index(int? siteId)
    {
        var query = db.SiteConfigurations
            .Include(c => c.Site)
            .Include(c => c.DataPlane)
            .AsQueryable();
        if (siteId.HasValue)
            query = query.Where(c => c.SiteId == siteId.Value);
        ViewBag.Sites = await db.Sites.OrderBy(s => s.Name).ToListAsync();
        ViewBag.FilterSiteId = siteId;
        return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var config = await db.SiteConfigurations
            .Include(c => c.Site)
            .Include(c => c.DataPlane)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (config == null) return NotFound();
        return View(config);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int siteId)
    {
        var site = await db.Sites
            .Include(s => s.DataPlanes)
            .Include(s => s.Services)
            .FirstOrDefaultAsync(s => s.Id == siteId);
        if (site == null) return NotFound();

        if (!site.DataPlanes.Any())
        {
            TempData["Error"] = "Site has no data planes assigned — cannot generate configuration.";
            return RedirectToAction("Details", "Sites", new { id = siteId });
        }

        int generated = 0, skipped = 0;

        foreach (var dataPlane in site.DataPlanes)
        {
            var dataPlaneRules = await db.AclRules
                .Include(r => r.SourceService).ThenInclude(s => s.Site)
                .Include(r => r.DestinationService).ThenInclude(s => s.Site)
                .Where(r => r.DataPlaneId == dataPlane.Id && r.IsEnabled &&
                            (r.SourceService.SiteId == siteId || r.DestinationService.SiteId == siteId))
                .ToListAsync();

            var candidate = await generator.GenerateAsync(site, dataPlane, dataPlaneRules);

            var lastConfig = await db.SiteConfigurations
                .Where(c => c.SiteId == siteId && c.DataPlaneId == dataPlane.Id)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            if (lastConfig?.ContentHash == candidate.ContentHash)
            {
                skipped++;
                continue;
            }

            var lastVersion = lastConfig?.Version ?? 0;
            candidate.Version = lastVersion + 1;
            candidate.Status = ConfigurationStatus.Sent;
            candidate.SentAt = DateTime.UtcNow;
            db.SiteConfigurations.Add(candidate);
            generated++;
        }

        await db.SaveChangesAsync();

        if (generated == 0)
            TempData["Info"] = $"No changes detected — all {skipped} data plane configuration(s) are up to date.";
        else
            TempData["Success"] = $"Generated and sent {generated} new configuration(s) for '{site.Name}'" +
                                  (skipped > 0 ? $" ({skipped} unchanged)" : "") + ". The Site Agent will see them on its next poll.";

        return RedirectToAction(nameof(Index), new { siteId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int id)
    {
        var config = await db.SiteConfigurations.Include(c => c.Site).FirstOrDefaultAsync(c => c.Id == id);
        if (config == null) return NotFound();
        if (config.Status != ConfigurationStatus.Draft)
        {
            TempData["Error"] = "Only Draft configurations can be sent.";
            return RedirectToAction(nameof(Details), new { id });
        }
        config.Status = ConfigurationStatus.Sent;
        config.SentAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = $"Configuration v{config.Version} marked as Sent.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var config = await db.SiteConfigurations.FindAsync(id);
        if (config == null) return NotFound();
        if (config.Status != ConfigurationStatus.Sent)
        {
            TempData["Error"] = "Only Sent configurations can be approved.";
            return RedirectToAction(nameof(Details), new { id });
        }
        config.Status = ConfigurationStatus.Approved;
        config.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Configuration approved.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkApplied(int id)
    {
        var config = await db.SiteConfigurations.FindAsync(id);
        if (config == null) return NotFound();
        config.Status = ConfigurationStatus.Applied;
        config.AppliedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Configuration marked as applied.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        var config = await db.SiteConfigurations.FindAsync(id);
        if (config == null) return NotFound();
        config.Status = ConfigurationStatus.Rejected;
        config.RejectionReason = reason;
        await db.SaveChangesAsync();
        TempData["Success"] = "Configuration rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> DownloadK8s(int id)
    {
        var config = await db.SiteConfigurations
            .Include(c => c.Site)
            .Include(c => c.DataPlane)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (config == null || config.K8sManifestYaml == null) return NotFound();
        var dpSlug = config.DataPlane != null ? $"-{SanitizeName(config.DataPlane.Name)}" : "";
        var bytes = System.Text.Encoding.UTF8.GetBytes(config.K8sManifestYaml);
        return File(bytes, "application/yaml", $"k8s-{SanitizeName(config.Site.Name)}{dpSlug}-v{config.Version}.yaml");
    }
}
