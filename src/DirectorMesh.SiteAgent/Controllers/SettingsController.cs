using System.Net;
using DirectorMesh.SiteAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace DirectorMesh.SiteAgent.Controllers;

public class SettingsController(AgentSettingsStore store, IHttpClientFactory httpFactory) : Controller
{
    public IActionResult Index()
    {
        var current = store.Current;
        return View(new SettingsViewModel
        {
            BaseUrl = current.BaseUrl,
            SiteId = current.SiteId,
            HasApiKey = !string.IsNullOrEmpty(current.ApiKey)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsViewModel model, string submit)
    {
        var current = store.Current;
        model.HasApiKey = !string.IsNullOrEmpty(current.ApiKey);

        // A blank API key field means "keep the existing key" so it never has to be re-displayed.
        var candidate = new DirectorMeshOptions
        {
            BaseUrl = model.BaseUrl?.Trim() ?? string.Empty,
            SiteId = model.SiteId,
            ApiKey = string.IsNullOrWhiteSpace(model.ApiKey) ? current.ApiKey : model.ApiKey.Trim()
        };

        if (!Uri.TryCreate(candidate.BaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            ModelState.AddModelError(nameof(model.BaseUrl), "Enter an absolute http(s) URL.");
        if (candidate.SiteId <= 0)
            ModelState.AddModelError(nameof(model.SiteId), "Site ID must be a positive number.");
        if (string.IsNullOrWhiteSpace(candidate.ApiKey))
            ModelState.AddModelError(nameof(model.ApiKey), "An API key is required.");
        if (!ModelState.IsValid)
            return View(model);

        var (ok, message) = await TestConnectionAsync(candidate);

        if (submit == "test")
        {
            model.TestOk = ok;
            model.TestMessage = message;
            return View(model);
        }

        await store.SaveAsync(candidate);
        if (ok)
        {
            TempData["Success"] = $"Settings saved. {message}";
            return RedirectToAction("Index", "Home");
        }

        TempData["Warning"] = $"Settings saved, but the connection test failed: {message}";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(bool Ok, string Message)> TestConnectionAsync(DirectorMeshOptions candidate)
    {
        try
        {
            var http = httpFactory.CreateClient(nameof(DirectorMeshClient));
            http.Timeout = TimeSpan.FromSeconds(15);
            var configs = await new DirectorMeshClient(http, candidate).GetConfigsAsync();
            return (true, $"Connected to Director-MESH as site {candidate.SiteId} ({configs.Count} data plane(s)).");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return (false, $"Director-MESH rejected the API key for site {candidate.SiteId}.");
        }
        catch (TaskCanceledException)
        {
            return (false, "Timed out waiting for Director-MESH.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}

public class SettingsViewModel
{
    public string? BaseUrl { get; set; }
    public int SiteId { get; set; }
    public string? ApiKey { get; set; }
    public bool HasApiKey { get; set; }
    public bool? TestOk { get; set; }
    public string? TestMessage { get; set; }
}
