﻿namespace GestionVentas.Models
{
    public class Empresa
    {

        public int EmpresaID { get; set; }
        public string Nombre { get; set; }
        public string NIT { get; set; }
        public string Giro { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public bool Estado { get; set; } 

    }
}
