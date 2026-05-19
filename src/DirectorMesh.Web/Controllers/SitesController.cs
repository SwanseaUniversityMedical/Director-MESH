using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using DirectorMesh.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class SitesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? dataPlaneId)
    {
        var query = db.Sites.Include(s => s.DataPlanes).Include(s => s.Services).AsQueryable();
        if (dataPlaneId.HasValue)
        {
            query = query.Where(s => s.DataPlanes.Any(dp => dp.Id == dataPlaneId.Value));
            ViewBag.FilterDataPlane = await db.DataPlanes.FindAsync(dataPlaneId.Value);
        }
        ViewBag.DataPlanes = await db.DataPlanes.OrderBy(d => d.Name).ToListAsync();
        return View(await query.OrderBy(s => s.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var site = await db.Sites
            .Include(s => s.DataPlanes)
            .Include(s => s.Services)
            .Include(s => s.Configurations.OrderByDescending(c => c.Version).Take(5))
            .FirstOrDefaultAsync(s => s.Id == id);
        if (site == null) return NotFound();
        return View(site);
    }

    public async Task<IActionResult> Create(int? dataPlaneId)
    {
        var vm = new SiteCreateEditViewModel();
        if (dataPlaneId.HasValue)
            vm.DataPlaneIds = new List<int> { dataPlaneId.Value };
        await PopulateDataPlanes(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SiteCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) { await PopulateDataPlanes(vm); return View(vm); }
        var site = new Site
        {
            Name = vm.Name,
            Description = vm.Description,
            Status = vm.Status,
            ApiKey = vm.ApiKey,
            CreatedAt = DateTime.UtcNow
        };
        if (vm.DataPlaneIds.Count > 0)
            site.DataPlanes = await db.DataPlanes.Where(dp => vm.DataPlaneIds.Contains(dp.Id)).ToListAsync();
        db.Sites.Add(site);
        await db.SaveChangesAsync();
        TempData["Success"] = $"Site '{site.Name}' created.";
        return RedirectToAction(nameof(Details), new { id = site.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var site = await db.Sites.Include(s => s.DataPlanes).FirstOrDefaultAsync(s => s.Id == id);
        if (site == null) return NotFound();
        var vm = new SiteCreateEditViewModel
        {
            Id = site.Id,
            Name = site.Name,
            Description = site.Description,
            ApiKey = site.ApiKey,
            DataPlaneIds = site.DataPlanes.Select(dp => dp.Id).ToList(),
            Status = site.Status
        };
        await PopulateDataPlanes(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SiteCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) { await PopulateDataPlanes(vm); return View(vm); }
        var site = await db.Sites.Include(s => s.DataPlanes).FirstOrDefaultAsync(s => s.Id == id);
        if (site == null) return NotFound();
        site.Name = vm.Name;
        site.Description = vm.Description;
        site.ApiKey = vm.ApiKey;
        site.Status = vm.Status;
        var newDataPlanes = await db.DataPlanes.Where(dp => vm.DataPlaneIds.Contains(dp.Id)).ToListAsync();
        site.DataPlanes.Clear();
        foreach (var dp in newDataPlanes)
            site.DataPlanes.Add(dp);
        await db.SaveChangesAsync();
        TempData["Success"] = $"Site '{site.Name}' updated.";
        return RedirectToAction(nameof(Details), new { id = site.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var site = await db.Sites.Include(s => s.DataPlanes).FirstOrDefaultAsync(s => s.Id == id);
        if (site == null) return NotFound();
        return View(site);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var site = await db.Sites.FindAsync(id);
        if (site != null)
        {
            db.Sites.Remove(site);
            await db.SaveChangesAsync();
            TempData["Success"] = "Site deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDataPlanes(SiteCreateEditViewModel vm)
    {
        vm.DataPlanes = (await db.DataPlanes.OrderBy(d => d.Name).ToListAsync())
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()));
    }
}
