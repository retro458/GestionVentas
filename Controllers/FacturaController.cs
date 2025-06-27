using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using GestionVentas.Models_Mongo;
using GestionVentas.Servicies;
using GestionVentas.Context;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;


using Newtonsoft.Json;


namespace GestionVentas.Controllers
{
    public class FacturaController : Controller
    {
        private readonly MongoServicie _mongoService;
        private readonly AppDbcontext _context;

        public FacturaController(MongoServicie mongoService, AppDbcontext context)
        {
            _mongoService = mongoService;
            _context = context;
        }
        [HttpPost]
        public IActionResult FirmarFactura()
        {
            var empresa = _context.Empresas.FirstOrDefault();
            var cliente = _context.Clientes.Include(c => c.Empresa).FirstOrDefault();
            var productos = _context.Producto.Take(2).ToList();

            var detalles = productos.Select(p => new FacturaJson.DetalleFactura
            {
                Producto = p.Nombre,
                Cantidad = 1, // Asignar una cantidad por defecto
                Precio = p.Precio
            }).ToList();

            decimal total = detalles.Sum(d => d.Precio * d.Cantidad);

            var factura = new FacturaJson
            {
                Emisor = empresa?.Nombre ?? "Desconocido",
                Cliente = cliente?.Nombre ?? "Desconocido",
                FechaEmision = DateTime.Now,
                Detalles = detalles,
                Total = total,
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
            // Serializar la factura a JSON y guardarla en TempData
            var jsonFactura = JsonConvert.SerializeObject(factura, Formatting.Indented);
            TempData["jsonFactura"] = jsonFactura;


            return RedirectToAction("Index");

        }

        public IActionResult Index()
        {
            return View();
        }
    


    [HttpPost]
        public async Task<IActionResult> EnviarAFakeHacienda()
        {

            var factura = _mongoService.FacturasFirmadas.Find(f => f.Estado == "Firmada").FirstOrDefault();

            if (factura == null)
            {
              TempData["Mensaje"] = "No hay facturas firmadas para enviar a Hacienda.";
                return RedirectToAction("Index");
            }

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://localhost:7013/api/Hacienda/EnviarFactura", factura);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                factura.Estado = "Enviada"; // Actualizar el estado de la factura
                await _mongoService.FacturasFirmadas.ReplaceOneAsync(f => f.Id == factura.Id, factura);
                TempData["Mensaje"] = $"Factura enviada a Hacienda";
            }
            else
            {
                TempData["Mensaje"] = "Error al enviar la factura a Hacienda.";
            }
            return RedirectToAction("Index");
        }

    }
}