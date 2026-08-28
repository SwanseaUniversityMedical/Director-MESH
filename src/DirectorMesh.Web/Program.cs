using DirectorMesh.Web.Data;
using DirectorMesh.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/signin";
    options.LogoutPath = "/signout";
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var kc = builder.Configuration.GetSection("Keycloak");
    options.Authority = kc["Authority"];
    options.ClientId = kc["ClientId"];
    options.ClientSecret = kc["ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

    // When running in Docker the container cannot reach localhost:8080 (the host-mapped
    // Keycloak port). Keycloak:InternalAuthority (e.g. http://keycloak:8080) is set via
    // docker-compose. The handler transparently rewrites backchannel calls to that URL
    // while the browser still redirects to the public localhost:8080 address.
    var internalAuthority = kc["InternalAuthority"];
    if (!string.IsNullOrEmpty(internalAuthority) && !string.IsNullOrEmpty(options.Authority))
    {
        var publicBase = new Uri(options.Authority).GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
        options.BackchannelHttpHandler = new KeycloakBackchannelHandler(publicBase, internalAuthority);
    }
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<INetBirdSettingsResolver, NetBirdSettingsResolver>();
builder.Services.AddScoped<INetBirdService, NetBirdService>();
builder.Services.AddScoped<IConfigurationGeneratorService, ConfigurationGeneratorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/signout-callback", async context =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties { RedirectUri = "/" });
});

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
