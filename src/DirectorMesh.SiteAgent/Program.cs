using DirectorMesh.SiteAgent.Services;

var builder = WebApplication.CreateBuilder(args);

var dmOptions = builder.Configuration.GetSection("DirectorMesh").Get<DirectorMeshOptions>()
    ?? throw new InvalidOperationException("DirectorMesh configuration section is missing.");

builder.Services.AddSingleton(dmOptions);

builder.Services.AddHttpClient<DirectorMeshClient>(http =>
{
    http.BaseAddress = new Uri(dmOptions.BaseUrl.TrimEnd('/') + "/");
    http.DefaultRequestHeaders.Add("X-Api-Key", dmOptions.ApiKey);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

builder.Services.AddControllersWithViews();

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
