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
            .Include(c => c.Site).ThenInclude(s => s.DataPlanes)
            .AsQueryable();
        if (siteId.HasValue)
            query = query.Where(c => c.SiteId == siteId.Value);
        ViewBag.Sites = await db.Sites.Include(s => s.DataPlanes).OrderBy(s => s.Name).ToListAsync();
        ViewBag.FilterSiteId = siteId;
        return View(await query.OrderByDescending(c => c.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var config = await db.SiteConfigurations
            .Include(c => c.Site).ThenInclude(s => s.DataPlanes)
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

        var dataPlaneIds = site.DataPlanes.Select(dp => dp.Id).ToList();
        var rules = await db.AclRules
            .Include(r => r.SourceService).ThenInclude(s => s.Site)
            .Include(r => r.DestinationService).ThenInclude(s => s.Site)
            .Where(r => dataPlaneIds.Contains(r.DataPlaneId) && r.IsEnabled &&
                        (r.SourceService.SiteId == siteId || r.DestinationService.SiteId == siteId))
            .ToListAsync();

        var lastVersion = await db.SiteConfigurations
            .Where(c => c.SiteId == siteId)
            .MaxAsync(c => (int?)c.Version) ?? 0;

        var config = await generator.GenerateAsync(site, rules);
        config.Version = lastVersion + 1;
        db.SiteConfigurations.Add(config);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Configuration v{config.Version} generated for site '{site.Name}'.";
        return RedirectToAction(nameof(Details), new { id = config.Id });
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
        TempData["Success"] = $"Configuration v{config.Version} marked as Sent. Approval token: {config.ApprovalToken}";
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
        var config = await db.SiteConfigurations.Include(c => c.Site).FirstOrDefaultAsync(c => c.Id == id);
        if (config == null || config.K8sManifestYaml == null) return NotFound();
        var bytes = System.Text.Encoding.UTF8.GetBytes(config.K8sManifestYaml);
        return File(bytes, "application/yaml", $"k8s-{SanitizeName(config.Site.Name)}-v{config.Version}.yaml");
    }
}
