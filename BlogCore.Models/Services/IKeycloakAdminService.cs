using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogCore.Models.ViewModels;

namespace BlogCore.Models.Services
{
    public interface IKeycloakAdminService
    {
        Task<string> CreateUserAsync(RegisterUsuarioViewModel model);
        Task<bool> AssignRoleAsync(string userId, string roleName);
    }
}