using System.Net.Http.Headers;
using System.Text;
using DirectorMesh.SiteAgent.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var dmOptions = builder.Configuration.GetSection("DirectorMesh").Get<DirectorMeshOptions>()
    ?? throw new InvalidOperationException("DirectorMesh configuration section is missing.");

var giteaOptions = builder.Configuration.GetSection("Gitea").Get<GiteaOptions>() ?? new GiteaOptions();

builder.Services.AddSingleton(dmOptions);
builder.Services.AddSingleton(giteaOptions);

builder.Services.AddHttpClient<DirectorMeshClient>(http =>
{
    http.BaseAddress = new Uri(dmOptions.BaseUrl.TrimEnd('/') + "/");
    http.DefaultRequestHeaders.Add("X-Api-Key", dmOptions.ApiKey);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
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
