using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using DirectorMesh.Web.Models.ViewModels;
using DirectorMesh.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class SettingsController(ApplicationDbContext db, INetBirdSettingsResolver settingsResolver) : Controller
{
    public async Task<IActionResult> NetBird()
    {
        var stored = await db.NetBirdSettings.FirstOrDefaultAsync();
        var vm = new NetBirdSettingsViewModel
        {
            ApiBase = stored?.ApiBase,
            ApiToken = stored?.ApiToken,
            DnsDomain = stored?.DnsDomain,
            UpdatedAt = stored?.UpdatedAt,
        };
        await PopulateEffective(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> NetBird(NetBirdSettingsViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateEffective(vm);
            return View(vm);
        }

        var settings = await db.NetBirdSettings.FirstOrDefaultAsync() ?? new NetBirdSettings();
        settings.ApiBase = string.IsNullOrWhiteSpace(vm.ApiBase) ? null : vm.ApiBase.Trim();
        settings.ApiToken = string.IsNullOrWhiteSpace(vm.ApiToken) ? null : vm.ApiToken.Trim();
        settings.DnsDomain = string.IsNullOrWhiteSpace(vm.DnsDomain) ? null : vm.DnsDomain.Trim();
        settings.UpdatedAt = DateTime.UtcNow;
        if (settings.Id == 0) db.NetBirdSettings.Add(settings);

        await db.SaveChangesAsync();
        TempData["Success"] = "NetBird settings updated.";
        return RedirectToAction(nameof(NetBird));
    }

    private async Task PopulateEffective(NetBirdSettingsViewModel vm)
    {
        var effective = await settingsResolver.ResolveAsync();
        vm.EffectiveApiBase = effective.ApiBase;
        vm.EffectiveApiTokenIsSet = !string.IsNullOrWhiteSpace(effective.ApiToken);
        vm.EffectiveDnsDomain = string.IsNullOrWhiteSpace(effective.DnsDomain) ? "(not set)" : effective.DnsDomain;
    }
}
