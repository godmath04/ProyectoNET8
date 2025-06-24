using BlogCore.Utilidades;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AuthController : Controller
{
    public IActionResult SignIn()
    {
        
        //return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "oidc");
        return Challenge(new AuthenticationProperties { RedirectUri = "/auth/postlogin" }, "oidc");
    }

    [AllowAnonymous]
    public async Task<IActionResult> PostLogin()
    {
        // 🧠 Obtener el token JWT emitido por Keycloak
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
        return SignOut(
            new AuthenticationProperties
            {
                RedirectUri = "/"
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            "oidc" // ✅ Este es el nombre que usaste en AddOpenIdConnect
            );

    }


}
