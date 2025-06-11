using Azure.Identity;
using BlogCore.Data;
using BlogCore.Models;
using BlogCore.Utilidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogCore.AccesoDatos.Data.Inicializador
{
    public class InicializadorBD : IInicializadorBD
    {
        private readonly ApplicationDbContext _bd;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //Creacion del constructor
        public InicializadorBD(ApplicationDbContext bd, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _bd = bd;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Inicializar()
        {
            // Aplicar migraciones pendientes
            if (_bd.Database.GetPendingMigrations().Any())
            {
                _bd.Database.Migrate();
            }

            // Crear roles si no existen
            if (!_roleManager.RoleExistsAsync(CNT.GestorDeTickets).Result)
                _roleManager.CreateAsync(new IdentityRole(CNT.GestorDeTickets)).Wait();
            if (!_roleManager.RoleExistsAsync(CNT.AgenteSoporte).Result)
                _roleManager.CreateAsync(new IdentityRole(CNT.AgenteSoporte)).Wait();
            if (!_roleManager.RoleExistsAsync(CNT.Solicitante).Result)
                _roleManager.CreateAsync(new IdentityRole(CNT.Solicitante)).Wait();

            // Crear usuario admin si no existe
            var usuario = _userManager.FindByEmailAsync("luispineda72@hotmail.com").Result;
            if (usuario == null)
            {
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "luispineda72@hotmail.com",
                    Email = "luispineda72@hotmail.com",
                    EmailConfirmed = true,
                    Nombre = "Administrador"
                }, "Admin@123").Wait();

                usuario = _bd.ApplicationUser.FirstOrDefault(u => u.Email == "luispineda72@hotmail.com");
            }

            // Asignar rol al usuario si aún no lo tiene
            if (!_userManager.IsInRoleAsync(usuario, CNT.GestorDeTickets).Result)
            {
                _userManager.AddToRoleAsync(usuario, CNT.GestorDeTickets).Wait();
            }
        }

    }
}
