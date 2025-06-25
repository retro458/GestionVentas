using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GestionVentas.Models
{
    public class Usuarios
    {
        public int UsuarioID { get; set; }
        public string NombreEmpleado { get; set; } 
        public string Usuario { get; set; }
        public string Contra { get; set; }
        public int RolID { get; set; }
        public bool Estado { get; set; }

        public Rol Roles { get; set; }

    }
}
