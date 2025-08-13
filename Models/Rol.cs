namespace GestionVentas.Models
{
    public class Rol
    {
        public int RolID { get; set; }
        public string NombreRol { get; set; }


        public ICollection<PermisoRol> permisoRol { get; set; } 
    }
}
