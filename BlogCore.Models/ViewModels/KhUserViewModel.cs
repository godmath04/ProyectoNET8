using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogCore.Models.ViewModels
{
    public class KhUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; } // este vendría de 'firstName'
        public string Apellido { get; set; } // este de 'lastName'
        public bool Enabled { get; set; } // este de 'enabled' en Keycloak
    }
}

