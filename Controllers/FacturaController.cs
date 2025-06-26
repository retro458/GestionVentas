using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using GestionVentas.Models_Mongo;
using GestionVentas.Servicies;

namespace GestionVentas.Controllers
{
    public class FacturaController : Controller
    {
        private readonly MongoServicie _mongoService;

        public FacturaController(MongoServicie mongoService)
        {
            _mongoService = mongoService;
        }
        [HttpPost]
        public IActionResult FirmarFactura()
        {
            var factura = new FacturaJson
            {
                Emisor = "Empresa XYZ",
                Cliente = "Cliente ABC",
                FechaEmision = DateTime.Now,
                Detalles = new List<FacturaJson.DetalleFactura>
                {
                    new FacturaJson.DetalleFactura { Producto = "Producto 1", Cantidad = 2, Precio = 50.00m },
                    new FacturaJson.DetalleFactura { Producto = "Producto 2", Cantidad = 1, Precio = 100.00m }
                },
                Total = 200.00m,


            };

            // Generar una firma digital para la factura
            string rwData = $"{factura.Emisor}|{factura.Cliente}|{factura.FechaEmision}|{factura.Total}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rwData));
                factura.Firma = Convert.ToHexString(hashBytes);

            }

            // Guardar la factura en MongoDB
            factura.Estado = "Firmada"; // Estado de la factura
            _mongoService.FacturasFirmadas.InsertOne(factura);
            // Redirigir a la vista de éxito o mostrar un mensaje
            TempData["Mensaje"] = "Factura firmada y guardada exitosamente.";

            return RedirectToAction("Index");

        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
