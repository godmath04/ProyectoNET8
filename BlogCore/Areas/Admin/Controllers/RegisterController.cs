using BlogCore.Models.Services;
using BlogCore.Models.ViewModels;
using BlogCore.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogCore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CNT.GestorDeTickets)]
    public class RegisterController : Controller
    {
        private readonly IKeycloakAdminService _keycloakAdminService;

        public RegisterController(IKeycloakAdminService keycloakAdminService)
        {
            _keycloakAdminService = keycloakAdminService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(RegisterUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var userId = await _keycloakAdminService.CreateUserAsync(model);
                if (!string.IsNullOrEmpty(userId))
                {
                    var rolAsignado = await _keycloakAdminService.AssignRoleAsync(userId, model.Rol);
                    if (rolAsignado)
                    {
                        TempData["success"] = "✅ Usuario creado y rol asignado correctamente.";
                        return RedirectToAction("Index", "Usuarios");
                    }
                    else
                    {
                        ModelState.AddModelError("", "❌ Usuario creado pero el rol no fue asignado.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "❌ No se pudo crear el usuario en Keycloak.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"❌ Error: {ex.Message}");
            }

            return View(model);
        }
    }
}
