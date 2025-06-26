using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace GestionVentas.Models_Mongo
{
    public class FacturaJson
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Emisor { get; set; }
        public string Cliente { get; set; }
        public DateTime FechaEmision { get; set; }
        public List<DetalleFactura> Detalles { get; set; }
        public decimal Total { get; set; }
        public string Firma { get; set; }
        public string Estado { get; set; } // "Firmada", "Pendiente", etc.

        public class DetalleFactura
        {
          public string Producto { get; set; }
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }

        }

    }
}
