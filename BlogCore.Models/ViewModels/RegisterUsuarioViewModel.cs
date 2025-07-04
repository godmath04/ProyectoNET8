using System.ComponentModel.DataAnnotations;

namespace BlogCore.Models.ViewModels
{
    public class RegisterUsuarioViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Rol { get; set; }
    }
}
