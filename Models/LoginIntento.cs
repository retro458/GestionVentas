namespace GestionVentas.Models
{
    public class LoginIntento
    {
        public int Id { get; set; }
        public string UsuarioIntentado { get; set; }
        public string IP { get; set; }
        public string Navegador { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Exitoso { get; set; }
        public string Motivo { get; set; }
    }
}
