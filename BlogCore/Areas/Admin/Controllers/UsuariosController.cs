using BlogCore.AccesoDatos.Data.Repository.IRepository;
using BlogCore.Models.Services;
using BlogCore.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogCore.Areas.Admin.Controllers
{
    [Authorize(Roles = CNT.GestorDeTickets)]
    [Area("Admin")]
    public class UsuariosController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private IKeycloakAdminService _kc;

        public UsuariosController(IContenedorTrabajo contenedorTrabajo, IKeycloakAdminService kc)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _kc = kc;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = (await _kc.GetUsersAsync()).ToList();

            var claims = (ClaimsIdentity)User.Identity;
            var currentId = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            usuarios = usuarios.Where(u => u.Id != currentId).ToList();

            return View(usuarios);


        }
        [Authorize(Roles = CNT.GestorDeTickets + "," + CNT.AgenteSoporte)]

        [Authorize(Roles = CNT.GestorDeTickets + "," + CNT.AgenteSoporte)]
        [HttpPost]
        public async Task<IActionResult> Bloquear(string id)
        {
            var success = await _kc.DisableUserAsync(id);
            TempData["success"] = success ? "Usuario bloqueado." : "Error al bloquear.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Desbloquear(string id)
        {
            var success = await _kc.EnableUserAsync(id);
            TempData["success"] = success ? "Usuario desbloqueado." : "Error al desbloquear.";
            return RedirectToAction(nameof(Index));
        }


    }
}
