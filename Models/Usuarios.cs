using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace GestionVentas.Models
{
    public class Usuarios
    {
        public int UsuarioID { get; set; }
        [Required(ErrorMessage ="El nombre del empleado es obligatorio")]
        public string NombreEmpleado { get; set; }
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Contra { get; set; }
        [Required(ErrorMessage = "El estado es obligatorio")]
        public bool Estado  { get; set; }
        [Required(ErrorMessage = "El rol es obligatorio")]
        public int RolID { get; set; }

        public Rol? Roles { get; set; }

    }
}
