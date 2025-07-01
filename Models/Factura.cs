namespace GestionVentas.Models
{
    public class Factura
    {
        public int FacturaID { get; set; }
        public int ClienteID { get; set; }
        public int EmpresaID { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; }
        public string Firmahash { get; set; }


    }
}
