using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WwExecutionWebMvc.Controllers;

/// <summary>
/// Public landing area. The app has a global "must be authenticated" fallback
/// policy, so anonymous pages opt out explicitly with <see cref="AllowAnonymousAttribute"/>.
/// </summary>
public sealed class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}
