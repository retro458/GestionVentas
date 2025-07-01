namespace GestionVentas.Models
{
    public class HistorialFactura
    {
        public int HistoriaFacturasID { get; set; }
        public int FacturaID { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public int UsuarioID { get; set; }

        public Factura? Factura { get; set; } // Relación con la entidad Factura
        public Usuarios? Usuario { get; set; } // Relación con la entidad Usuario
    }
}
