using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using WwExecutionWebMvc.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Authentication + token acquisition
// ---------------------------------------------------------------------------
// 1. AddMicrosoftIdentityWebApp wires up OpenID Connect Authorization Code flow
//    against Entra ID (this app is a CONFIDENTIAL client — it has a secret).
// 2. EnableTokenAcquisitionToCallDownstreamApi turns on MSAL's confidential
//    token cache so the auth-code-acquired token can be exchanged silently for
//    downstream access tokens (delegated, on the signed-in user's behalf).
// 3. AddDownstreamApi registers a named "WwExecution" downstream API. The
//    IDownstreamApi service auto-acquires the user_impersonation access token
//    and injects it as a Bearer header on every outbound call.
// 4. AddInMemoryTokenCaches caches tokens server-side per user.
//    NOTE: For production / scaled-out deployments use a DISTRIBUTED cache
//    (AddDistributedTokenCaches + Redis/SQL) so tokens survive restarts and are
//    shared across instances.
var initialScopes = builder.Configuration
    .GetSection("WwExecution:Scopes").Get<string[]>() ?? Array.Empty<string>();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddDownstreamApi(WwExecutionService.DownstreamApiName,
        builder.Configuration.GetSection("WwExecution"))
    .AddInMemoryTokenCaches();
    // Production alternative:
    // .AddDistributedTokenCaches();
    // builder.Services.AddStackExchangeRedisCache(o => o.Configuration = "<redis>");

// Require an authenticated user by default across the app; opt out per-action
// with [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ---------------------------------------------------------------------------
// MVC + Microsoft Identity UI (sign-in / sign-out / consent pages)
// ---------------------------------------------------------------------------
builder.Services
    .AddControllersWithViews()
    .AddMicrosoftIdentityUI();

// Engine client. The typed service depends on IDownstreamApi for secure/services
// calls (token auto-injected) and a plain HttpClient for anonymous public calls.
builder.Services.AddHttpClient();
builder.Services.AddScoped<IWwExecutionService, WwExecutionService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
