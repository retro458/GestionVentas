using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace GestionVentas.Models_Mongo
{
    public class FacturaJson
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public Identificacion Identificacion { get; set; }
        public Emisor Emisor { get; set; }
        public Receptor Receptor { get; set; }
        public List<CuerpoDocumento> CuerpoDocumento { get; set; }
        public Resumen Resumen { get; set; }
        public Extensiones Extensiones { get; set; }
        public string Firma { get; set; }
        public string Estado { get; set; } 
        public int? FacturaSQLID { get; set; } // Relación con SQL
    }

    public class Identificacion
    {
        public string Version { get; set; } = "1";
        public string Ambiente { get; set; } = "00"; // 00 = pruebas
        public string TipoDte { get; set; } = "01"; // 01 = CC Fiscal
        public string NumeroControl { get; set; }
        public DateTime FechaHoraEmision { get; set; }
        public string TipoMoneda { get; set; } = "USD";
    }

    public class Emisor
    {
        public string NIT { get; set; }
        public string Nombre { get; set; }
        public string NombreComercial { get; set; }
        public string ActividadEconomica { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string Direccion { get; set; }
    }

    public class Receptor
    {
        public string NIT { get; set; }
        public string Nombre { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string Direccion { get; set; }
    }

    public class CuerpoDocumento
    {
        public int Cantidad { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal VentasNoSujetas { get; set; } = 0;
        public decimal VentasExentas { get; set; } = 0;
        public decimal VentasGravadas { get; set; }
    }

    public class Resumen
    {
        public decimal TotalNoSuj { get; set; }
        public decimal TotalExenta { get; set; }
        public decimal TotalGravada { get; set; }
        public decimal SubTotalVentas { get; set; }
        public decimal IVA { get; set; }
        public decimal TotalPagar { get; set; }
    }

    public class Extensiones
    {
        public string NombreResponsable { get; set; }
        public string CorreoResponsable { get; set; }
    }

}
