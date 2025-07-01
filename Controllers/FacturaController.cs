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

         
            // guardar la factura en la base de datos SQL 
            var nuevaFactura = new Models.Factura
            {
                ClienteID = cliente.ClienteID,
                EmpresaID = empresa.EmpresaID,
                Fecha = factura.FechaEmision,
                Total = factura.Total,
                Estado = "Firmada",
                Firmahash = factura.Firma
            };
            _context.Factura.Add(nuevaFactura);
            _context.SaveChanges();
            // Asignar el id sql a la factura de mongo
            factura.FacturaSQLID = nuevaFactura.FacturaID;
            TempData["FacturaSQLID"] = nuevaFactura.FacturaID;
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

            if (TempData["FacturaSQLID"] == null)
            {
                TempData["Mensaje"] = "No se encontro el ID de la facutra para enviar.";
                return RedirectToAction("Index");
            }

            int facturaId = Convert.ToInt32(TempData["FacturaSQLID"]);
            var factura = _mongoService.FacturasFirmadas
                .Find(f => f.FacturaSQLID == facturaId)
                .FirstOrDefault();
            if (factura == null)
            {
                TempData["Mensaje"] = "No se encontro la factura firmada correspondiente.";
                return RedirectToAction("Index");
            }

                using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://localhost:7013/api/Hacienda/EnviarFactura", factura);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                factura.Estado = "Enviada"; // Actualizar el estado de la factura
                // Actualizar la factura con el resultado de Hacienda para mongo
                var facturaClon = new FacturaJson
                {
                    Emisor = factura.Emisor,
                    Cliente = factura.Cliente,
                    FechaEmision = factura.FechaEmision,
                    Detalles = factura.Detalles,
                    Total = factura.Total,
                    Firma = factura.Firma, // Asignar la firma recibida de Hacienda
                    Estado = factura.Estado,
                    FacturaSQLID = factura.FacturaSQLID // Mantener el ID de SQL si existe
                };

                // Guardar la factura actualizada en MongoDB
                await _mongoService.FacturasAceptadas.InsertOneAsync(facturaClon);
                //guardar en historial de facturas en sql
                if(factura.FacturaSQLID.HasValue)
                {
                    _context.HistorialFacturas.Add(new Models.HistorialFactura
                    {
                        FacturaID = factura.FacturaSQLID.Value,
                        FechaMovimiento = DateTime.Now,
                        EstadoAnterior = "Firmada",
                        EstadoNuevo = "Aceptada",
                        UsuarioID = 1
                    });
                    TempData["MensajeHistorial"] = "Factura guardada en historial de facturas.";
                    _context.SaveChanges();
                }
                else
                {
                    TempData["MensajeHistorial"] = "Factura no tiene ID de SQL para guardar en historial.";
                }
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