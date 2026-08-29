using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using DirectorMesh.Web.Models.ViewModels;
using DirectorMesh.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class NetworkServicesController(ApplicationDbContext db, INetBirdService netBirdService) : Controller
{
    public async Task<IActionResult> Index(int? siteId)
    {
        var query = db.NetworkServices.Include(s => s.Site).ThenInclude(s => s.DataPlanes).AsQueryable();
        if (siteId.HasValue)
        {
            query = query.Where(s => s.SiteId == siteId.Value);
            ViewBag.FilterSite = await db.Sites.FindAsync(siteId.Value);
        }
        ViewBag.Sites = await db.Sites.Include(s => s.DataPlanes).OrderBy(s => s.Name).ToListAsync();
        return View(await query.OrderBy(s => s.Site.Name).ThenBy(s => s.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var svc = await db.NetworkServices
            .Include(s => s.Site).ThenInclude(s => s.DataPlanes)
            .Include(s => s.SourceAclRules).ThenInclude(r => r.DestinationService).ThenInclude(s => s.Site)
            .Include(s => s.DestinationAclRules).ThenInclude(r => r.SourceService).ThenInclude(s => s.Site)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (svc == null) return NotFound();
        return View(svc);
    }

    public async Task<IActionResult> Create(int? siteId)
    {
        var vm = new NetworkServiceCreateEditViewModel { SiteId = siteId ?? 0 };
        await PopulateSites(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NetworkServiceCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) { await PopulateSites(vm); return View(vm); }
        var svc = new NetworkService
        {
            Name = vm.Name,
            Description = vm.Description,
            SiteId = vm.SiteId,
            Direction = vm.Direction,
            Protocol = vm.Protocol,
            Port = vm.Port,
            NetBirdPeerName = vm.NetBirdPeerName,
            NetBirdDnsName = vm.NetBirdDnsName,
            NetBirdIpAddress = vm.NetBirdIpAddress,
            NetBirdSetupKey = vm.NetBirdSetupKey,
            LocalEndpoint = vm.LocalEndpoint,
            RemoteNetBirdDnsName = vm.RemoteNetBirdDnsName,
            IsEnabled = vm.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };
        db.NetworkServices.Add(svc);
        await db.SaveChangesAsync();
        TempData["Success"] = $"Service '{svc.Name}' created.";
        return RedirectToAction(nameof(Details), new { id = svc.Id });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var svc = await db.NetworkServices.FindAsync(id);
        if (svc == null) return NotFound();
        var vm = new NetworkServiceCreateEditViewModel
        {
            Id = svc.Id,
            Name = svc.Name,
            Description = svc.Description,
            SiteId = svc.SiteId,
            Direction = svc.Direction,
            Protocol = svc.Protocol,
            Port = svc.Port,
            NetBirdPeerName = svc.NetBirdPeerName,
            NetBirdDnsName = svc.NetBirdDnsName,
            NetBirdIpAddress = svc.NetBirdIpAddress,
            NetBirdSetupKey = svc.NetBirdSetupKey,
            LocalEndpoint = svc.LocalEndpoint,
            RemoteNetBirdDnsName = svc.RemoteNetBirdDnsName,
            IsEnabled = svc.IsEnabled
        };
        await PopulateSites(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NetworkServiceCreateEditViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) { await PopulateSites(vm); return View(vm); }
        var svc = await db.NetworkServices.FindAsync(id);
        if (svc == null) return NotFound();
        svc.Name = vm.Name;
        svc.Description = vm.Description;
        svc.SiteId = vm.SiteId;
        svc.Direction = vm.Direction;
        svc.Protocol = vm.Protocol;
        svc.Port = vm.Port;
        svc.NetBirdPeerName = vm.NetBirdPeerName;
        svc.NetBirdDnsName = vm.NetBirdDnsName;
        svc.NetBirdIpAddress = vm.NetBirdIpAddress;
        svc.NetBirdSetupKey = vm.NetBirdSetupKey;
        svc.LocalEndpoint = vm.LocalEndpoint;
        svc.RemoteNetBirdDnsName = vm.RemoteNetBirdDnsName;
        svc.IsEnabled = vm.IsEnabled;
        await db.SaveChangesAsync();
        TempData["Success"] = $"Service '{svc.Name}' updated.";
        return RedirectToAction(nameof(Details), new { id = svc.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var svc = await db.NetworkServices.Include(s => s.Site).FirstOrDefaultAsync(s => s.Id == id);
        if (svc == null) return NotFound();
        return View(svc);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var svc = await db.NetworkServices.FindAsync(id);
        if (svc != null)
        {
            db.NetworkServices.Remove(svc);
            await db.SaveChangesAsync();
            TempData["Success"] = "Service deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNetBirdPeer(int id, int dataPlaneId)
    {
        var svc = await db.NetworkServices
            .Include(s => s.Site).ThenInclude(s => s.DataPlanes)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (svc == null) return NotFound();

        var dataPlane = svc.Site.DataPlanes.FirstOrDefault(dp => dp.Id == dataPlaneId);
        if (dataPlane == null)
        {
            TempData["Error"] = "Select a valid data plane for this site.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (string.IsNullOrWhiteSpace(dataPlane.NetBirdNetworkId))
        {
            TempData["Error"] = $"Data plane '{dataPlane.Name}' has no NetBird Network ID configured.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var peerName = $"{dataPlane.NetBirdNetworkId}-{svc.Name}";
        var setupKey = await netBirdService.CreateSetupKeyAsync(peerName);
        if (setupKey == null)
        {
            TempData["Error"] = "Failed to create the NetBird setup key. Check the NetBird settings and logs.";
            return RedirectToAction(nameof(Details), new { id });
        }

        svc.NetBirdPeerName = peerName;
        svc.NetBirdSetupKey = setupKey;
        await db.SaveChangesAsync();
        TempData["Success"] = $"NetBird setup key created for peer '{peerName}'.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateSites(NetworkServiceCreateEditViewModel vm)
    {
        vm.Sites = (await db.Sites.Include(s => s.DataPlanes).OrderBy(s => s.Name).ToListAsync())
            .Select(s => new SelectListItem(
                $"{s.Name} ({string.Join(", ", s.DataPlanes.Select(dp => dp.Name))})",
                s.Id.ToString()));
    }
}
