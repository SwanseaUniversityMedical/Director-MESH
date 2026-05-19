using DirectorMesh.Web.Data;
using DirectorMesh.Web.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DirectorMesh.Web.Controllers;

[Authorize]
public class DataPlanesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() =>
        View(await db.DataPlanes.Include(d => d.Sites).OrderBy(d => d.Name).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var dp = await db.DataPlanes
            .Include(d => d.Sites).ThenInclude(s => s.Services)
            .Include(d => d.AclRules).ThenInclude(r => r.SourceService).ThenInclude(s => s.Site)
            .Include(d => d.AclRules).ThenInclude(r => r.DestinationService).ThenInclude(s => s.Site)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (dp == null) return NotFound();
        return View(dp);
    }

    public IActionResult Create() => View(new DataPlane { Name = string.Empty });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DataPlane model)
    {
        if (!ModelState.IsValid) return View(model);
        model.CreatedAt = DateTime.UtcNow;
        db.DataPlanes.Add(model);
        await db.SaveChangesAsync();
        TempData["Success"] = $"Data plane '{model.Name}' created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dp = await db.DataPlanes.FindAsync(id);
        if (dp == null) return NotFound();
        return View(dp);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DataPlane model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Entry(model).State = EntityState.Modified;
        db.Entry(model).Property(x => x.CreatedAt).IsModified = false;
        await db.SaveChangesAsync();
        TempData["Success"] = $"Data plane '{model.Name}' updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var dp = await db.DataPlanes.Include(d => d.Sites).FirstOrDefaultAsync(d => d.Id == id);
        if (dp == null) return NotFound();
        return View(dp);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dp = await db.DataPlanes.FindAsync(id);
        if (dp != null)
        {
            db.DataPlanes.Remove(dp);
            await db.SaveChangesAsync();
            TempData["Success"] = "Data plane deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
