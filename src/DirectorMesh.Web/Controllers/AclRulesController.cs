using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using DirectorMesh.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class AclRulesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? dataPlaneId)
    {
        var query = db.AclRules
            .Include(r => r.DataPlane)
            .Include(r => r.SourceService).ThenInclude(s => s.Site)
            .Include(r => r.DestinationService).ThenInclude(s => s.Site)
            .AsQueryable();
        if (dataPlaneId.HasValue)
            query = query.Where(r => r.DataPlaneId == dataPlaneId.Value);
        ViewBag.DataPlanes = await db.DataPlanes.OrderBy(d => d.Name).ToListAsync();
        ViewBag.FilterDataPlaneId = dataPlaneId;
        return View(await query.OrderBy(r => r.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var rule = await db.AclRules
            .Include(r => r.DataPlane)
            .Include(r => r.SourceService).ThenInclude(s => s.Site)
            .Include(r => r.SourceService).ThenInclude(s => s.DataPlane)
            .Include(r => r.DestinationService).ThenInclude(s => s.Site)
            .Include(r => r.DestinationService).ThenInclude(s => s.DataPlane)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rule == null) return NotFound();
        return View(rule);
    }

    public async Task<IActionResult> Create(int? dataPlaneId)
    {
        var vm = new AclRuleCreateEditViewModel { DataPlaneId = dataPlaneId ?? 0 };
        await PopulateDropdowns(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AclRuleCreateEditViewModel vm)
    {
        await ValidateServices(vm);
        if (!ModelState.IsValid) { await PopulateDropdowns(vm); return View(vm); }
        var rule = new AclRule
        {
            Name = vm.Name,
            Description = vm.Description,
            DataPlaneId = vm.DataPlaneId,
            SourceServiceId = vm.SourceServiceId,
            DestinationServiceId = vm.DestinationServiceId,
            Action = vm.Action,
            IsEnabled = vm.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };
        db.AclRules.Add(rule);
        await db.SaveChangesAsync();
        TempData["Success"] = $"ACL rule '{rule.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var rule = await db.AclRules.FindAsync(id);
        if (rule == null) return NotFound();
        var vm = new AclRuleCreateEditViewModel
        {
            Id = rule.Id,
            Name = rule.Name,
            Description = rule.Description,
            DataPlaneId = rule.DataPlaneId,
            SourceServiceId = rule.SourceServiceId,
            DestinationServiceId = rule.DestinationServiceId,
            Action = rule.Action,
            IsEnabled = rule.IsEnabled
        };
        await PopulateDropdowns(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AclRuleCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        await ValidateServices(vm);
        if (!ModelState.IsValid) { await PopulateDropdowns(vm); return View(vm); }
        var rule = await db.AclRules.FindAsync(id);
        if (rule == null) return NotFound();
        rule.Name = vm.Name;
        rule.Description = vm.Description;
        rule.DataPlaneId = vm.DataPlaneId;
        rule.SourceServiceId = vm.SourceServiceId;
        rule.DestinationServiceId = vm.DestinationServiceId;
        rule.Action = vm.Action;
        rule.IsEnabled = vm.IsEnabled;
        await db.SaveChangesAsync();
        TempData["Success"] = $"ACL rule '{rule.Name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var rule = await db.AclRules
            .Include(r => r.DataPlane)
            .Include(r => r.SourceService).ThenInclude(s => s.Site)
            .Include(r => r.DestinationService).ThenInclude(s => s.Site)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (rule == null) return NotFound();
        return View(rule);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var rule = await db.AclRules.FindAsync(id);
        if (rule != null)
        {
            db.AclRules.Remove(rule);
            await db.SaveChangesAsync();
            TempData["Success"] = "ACL rule deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(AclRuleCreateEditViewModel vm)
    {
        vm.DataPlanes = (await db.DataPlanes.OrderBy(d => d.Name).ToListAsync())
            .Select(d => new SelectListItem(d.Name, d.Id.ToString()));

        var allServices = await db.NetworkServices
            .Include(s => s.Site).ThenInclude(s => s.DataPlanes)
            .OrderBy(s => s.Site.Name).ThenBy(s => s.Name)
            .ToListAsync();

        vm.OutboundServices = allServices
            .Where(s => s.Direction == ServiceDirection.Outbound)
            .Select(s => new SelectListItem($"{s.Site.Name} → {s.Name} ({s.Protocol}:{s.Port})", s.Id.ToString()));

        vm.InboundServices = allServices
            .Where(s => s.Direction == ServiceDirection.Inbound)
            .Select(s => new SelectListItem($"{s.Site.Name} ← {s.Name} ({s.Protocol}:{s.Port})", s.Id.ToString()));
    }

    private async Task ValidateServices(AclRuleCreateEditViewModel vm)
    {
        if (vm.SourceServiceId > 0)
        {
            var src = await db.NetworkServices.FindAsync(vm.SourceServiceId);
            if (src?.Direction != ServiceDirection.Outbound)
                ModelState.AddModelError(nameof(vm.SourceServiceId), "Source service must be an Outbound service.");
            else if (src.DataPlaneId != vm.DataPlaneId)
                ModelState.AddModelError(nameof(vm.SourceServiceId), "Source service is not on the selected data plane.");
        }
        if (vm.DestinationServiceId > 0)
        {
            var dst = await db.NetworkServices.FindAsync(vm.DestinationServiceId);
            if (dst?.Direction != ServiceDirection.Inbound)
                ModelState.AddModelError(nameof(vm.DestinationServiceId), "Destination service must be an Inbound service.");
            else if (dst.DataPlaneId != vm.DataPlaneId)
                ModelState.AddModelError(nameof(vm.DestinationServiceId), "Destination service is not on the selected data plane.");
        }
    }
}
