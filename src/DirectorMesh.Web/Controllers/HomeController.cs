using DirectorMesh.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.DataPlaneCount = await db.DataPlanes.CountAsync();
        ViewBag.SiteCount = await db.Sites.CountAsync();
        ViewBag.ServiceCount = await db.NetworkServices.CountAsync();
        ViewBag.AclRuleCount = await db.AclRules.CountAsync();
        ViewBag.PendingConfigurations = await db.SiteConfigurations
            .CountAsync(c => c.Status == Models.Domain.ConfigurationStatus.Sent);
        return View();
    }
}
