using System.Net.Http.Headers;
using System.Text;
using DirectorMesh.SiteAgent.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Settings saved from the Settings screen are layered last so they override
// appsettings and environment variables. In Docker this lives on the agent_data volume.
var settingsPath = builder.Configuration["AgentSettingsPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "data", "agent-settings.json");
builder.Configuration.AddJsonFile(settingsPath, optional: true, reloadOnChange: false);

var dmOptions = builder.Configuration.GetSection("DirectorMesh").Get<DirectorMeshOptions>() ?? new DirectorMeshOptions();

var giteaOptions = builder.Configuration.GetSection("Gitea").Get<GiteaOptions>() ?? new GiteaOptions();

builder.Services.AddSingleton(new AgentSettingsStore(dmOptions, settingsPath));
builder.Services.AddTransient(sp => sp.GetRequiredService<AgentSettingsStore>().Current);
builder.Services.AddSingleton(giteaOptions);

builder.Services.AddHttpClient<DirectorMeshClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddHttpClient<GiteaClient>(http =>
{
    http.BaseAddress = new Uri(giteaOptions.BaseUrl.TrimEnd('/') + "/");
    var credentials = Convert.ToBase64String(
        Encoding.UTF8.GetBytes($"{giteaOptions.Username}:{giteaOptions.Password}"));
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
});

builder.Services.AddScoped<GitopsService>();
builder.Services.AddControllersWithViews();

// Persist Data Protection keys so antiforgery tokens survive container restarts.
// In Docker the keys directory should be a named volume (see docker-compose.siteagent.yml).
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("DirectorMesh.SiteAgent");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.MapDefaultControllerRoute();

app.Run();
