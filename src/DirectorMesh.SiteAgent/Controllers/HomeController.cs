using DirectorMesh.SiteAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace DirectorMesh.SiteAgent.Controllers;

public class HomeController(DirectorMeshClient client, GitopsService gitops) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            var configs = await client.GetConfigsAsync();
            return View(configs);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Could not reach Director-MESH: {ex.Message}";
            return View(new List<DataPlaneConfigResponse>());
        }
    }

    public async Task<IActionResult> Config(int dataPlaneId)
    {
        var configs = await client.GetConfigsAsync();
        var dp = configs.FirstOrDefault(c => c.DataPlaneId == dataPlaneId);
        if (dp?.Pending == null) return RedirectToAction(nameof(Index));
        return View(dp);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int configId, string? notes)
    {
        var result = await client.ApproveAsync(configId, notes);
        if (result != null)
        {
            try
            {
                await gitops.PushApprovedConfigAsync(result);
                TempData["Success"] = $"Configuration v{result.Version} approved and pushed to Gitea.";
            }
            catch (Exception ex)
            {
                TempData["Success"] = $"Configuration v{result.Version} approved.";
                TempData["Warning"] = $"Gitea push failed: {ex.Message}";
            }
        }
        else
        {
            TempData["Error"] = "Approval failed — config may no longer be in Sent status.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int configId, string reason)
    {
        await client.RejectAsync(configId, reason);
        TempData["Success"] = "Configuration rejected.";
        return RedirectToAction(nameof(Index));
    }
}
