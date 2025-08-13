namespace GestionVentas.Models
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public string Nombre { get; set; }
        public string NIT { get; set; }
        public string DUI { get; set; }
        public string Telefono { get; set; }
        public int EmpresaID { get; set; }

        public Empresa? Empresa { get; set; }
    }
}
