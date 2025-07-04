using Microsoft.AspNetCore.Mvc;
using BlogCore.Models.ViewModels;
using BlogCore.Models.Services;
using BlogCore.Utilidades;

namespace BlogCore.Areas.Cliente.Controllers
{
    [Area("Cliente")]
    public class RegistroController : Controller
    {
        private readonly IKeycloakAdminService _kc;

        public RegistroController(IKeycloakAdminService kc)
        {
            _kc = kc;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Index(RegisterUsuarioViewModel model)
        {
            Console.WriteLine($"📥 Registro POST recibido: Email={model.Email}, Nombre={model.Nombre}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState inválido");
                return View(model);
            }

            try
            {
                model.Rol = CNT.Solicitante;
                Console.WriteLine("🚀 Llamando a CreateUserAsync...");
                var userId = await _kc.CreateUserAsync(model);

                Console.WriteLine($"🛠️ CreateUserAsync devolvió: {userId}");

                if (!string.IsNullOrEmpty(userId))
                {
                    await _kc.AssignRoleAsync(userId, CNT.Solicitante);
                    TempData["success"] = "Registro exitoso. Ahora puedes iniciar sesión.";
                    return RedirectToAction("Index", "Home", new { area = "Cliente" });
                }

                Console.WriteLine("❌ userId es null o vacío");
                ModelState.AddModelError("", "Error al crear el usuario en Keycloak.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception: " + ex);
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(model);
        }

    }

}
