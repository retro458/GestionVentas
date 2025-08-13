namespace GestionVentas.Models
{
    public class PermisoRol
    {
        public int PermisoRolID { get; set; }
        public int RolID { get; set; }
        public int PermisoID { get; set; }

        public Permiso? Permiso { get; set; }
    }
}
