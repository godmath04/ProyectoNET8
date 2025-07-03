using BlogCore.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AuthController : Controller
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult SignIn()
    {
        return Challenge(new AuthenticationProperties { RedirectUri = "/auth/postlogin" }, "oidc");
    }

    [AllowAnonymous]
    public async Task<IActionResult> PostLogin()
    {
        var idToken = await HttpContext.GetTokenAsync("id_token");
        Console.WriteLine($"🔐 ID Token: {idToken}");

        Console.WriteLine("🧾— BEGIN de TODOS LOS CLAIMS —🧾");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"🔎 {claim.Type} = {claim.Value}");
        }
        Console.WriteLine("🧾— END de TODOS LOS CLAIMS —🧾");

        Console.WriteLine($"🔍 User.IsInRole('GestorDeTickets') = {User.IsInRole(CNT.GestorDeTickets)}");

        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(CNT.GestorDeTickets))
                return RedirectToAction("Index", "Articulos", new { area = "Admin" });
            if (User.IsInRole(CNT.AgenteSoporte))
                return RedirectToAction("Index", "Articulos", new { area = "Admin" });
            if (User.IsInRole(CNT.Solicitante))
                return RedirectToAction("Index", "Home", new { area = "Cliente" });
        }

        Console.WriteLine("⚠️ Usuario sin rol esperado. Redirigiendo a acceso denegado.");
        return RedirectToAction("AccessDenied", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignOut()
    {
        var keycloakLogoutUrl = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/logout";
        var postLogoutUri = Url.Action("Index", "Home", new { area = "" }, Request.Scheme);

        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = postLogoutUri
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            "oidc"
        );
    }
}
