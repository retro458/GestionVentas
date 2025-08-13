using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using GestionVentas.Models_Mongo;
using GestionVentas.Servicies;
using GestionVentas.Context;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;


using Newtonsoft.Json;
using Rotativa.AspNetCore;
using GestionVentas.ViewsModels;
using System.Text.Json;


namespace GestionVentas.Controllers
{
    public class FacturaController : Controller
    {
        private readonly MongoServicie _mongoService;
        private readonly AppDbcontext _context;
        private readonly EmailService _emailService;
        public FacturaController(MongoServicie mongoService, AppDbcontext context, EmailService emailService)
        {
            _mongoService = mongoService;
            _context = context;
            _emailService = emailService;
        }

       

        [HttpPost]
        public IActionResult FirmarFactura()
        {
            var empresa = _context.Empresas?.FirstOrDefault();
            var cliente = _context.Clientes.Include(c => c.Empresa).FirstOrDefault();
            var productos = _context.Producto.Take(2).ToList();

            // generar cuerpo de la factura
            var cuerpo = productos.Select(p => new CuerpoDocumento
            {
                Cantidad = 1, // Asignar una cantidad por defecto
                Descripcion = p.Nombre,
                PrecioUnitario = p.Precio,
                VentasGravadas = p.Precio, // Asignar el precio del producto como ventas gravadas
            }).ToList();

            decimal totalGravada = cuerpo.Sum(c => c.VentasGravadas);
            decimal iva = totalGravada * 0.13m; // Asumiendo un IVA del 13%
            decimal totalPagar = totalGravada + iva;

            // armar la factura
            var factura = new FacturaJson
            {
                Identificacion = new Identificacion
                {
                    NumeroControl = Guid.NewGuid().ToString().Substring(0, 10).ToUpper(), //Generar un número de control único
                    FechaHoraEmision = DateTime.Now,
                },

                Emisor = new Emisor
                {
                    NIT = empresa?.NIT,
                    Nombre = empresa?.Nombre,
                    NombreComercial = empresa?.Nombre,
                    ActividadEconomica = empresa.Giro,
                    Departamento = "San Salvador",
                    Municipio = "San Salvador",
                    Direccion = empresa.Direccion
                },

                Receptor = new Receptor
                {
                    NIT = cliente?.NIT,
                    Nombre = cliente?.Nombre,
                    Departamento = "La Libertad",
                    Municipio = "San Tecla",
                    Direccion = "Calle 123"
                },
                CuerpoDocumento = cuerpo,
               Resumen = new Resumen
               {
                   TotalGravada = totalGravada,
                   TotalNoSuj = 0,
                   TotalExenta = 0,
                   SubTotalVentas = totalGravada,
                   IVA = iva,
                   TotalPagar = totalPagar
               },
               Extensiones = new Extensiones
               {
                   NombreResponsable = empresa?.Nombre,
                   CorreoResponsable = "Facturas@empresa.com"
               },

            };

            // Generar una firma digital para la factura
            string rwData = $"{factura.Emisor.NIT}|{factura.Receptor.NIT}|{factura.Resumen.TotalPagar}|{factura.Identificacion.FechaHoraEmision}";
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
                Fecha = factura.Identificacion.FechaHoraEmision,
                Total = totalPagar,
                Estado = "Firmada",
                Firmahash = factura.Firma
            };
            _context.Factura.Add(nuevaFactura);
            _context.SaveChanges();
            // Asignar el id sql a la factura de mongo
            factura.FacturaSQLID = nuevaFactura.FacturaID;
            HttpContext.Session.SetInt32("FacturaSQLID", nuevaFactura.FacturaID);
            
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

    


    [HttpPost]
        public async Task<IActionResult> EnviarAFakeHacienda()
        {

            int? facturaId = HttpContext.Session.GetInt32("FacturaSQLID");
            if (facturaId == null)
            {
                TempData["Mensaje"] = "No se encontró el ID de la factura para enviar.";
                return RedirectToAction("Index");
            }

            // Buscar la factura firmada en MongoDB
            var factura = _mongoService.FacturasFirmadas
                .Find(f => f.FacturaSQLID == facturaId)
                .FirstOrDefault();
            if (factura == null)
            {
                TempData["Mensaje"] = "No se encontro la factura firmada correspondiente.";
                return RedirectToAction("Index");
            }
            try
            {
                using var client = new HttpClient();
                var response = await client.PostAsJsonAsync("https://localhost:7013/api/Hacienda/EnviarFactura", factura);
                if (response.IsSuccessStatusCode)
                {
                    var jsonElement = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (jsonElement.TryGetProperty("numControl", out var controlElement))
                    {
                        factura.Identificacion.NumeroControl = controlElement.GetString();
                    }
                    else
                    {
                        factura.Identificacion.NumeroControl = Guid.NewGuid().ToString().Substring(0, 10).ToUpper();
                    }
                    // Actualizar el estado de la factura a "Aceptada"
                    factura.Firma = factura.Firma; // Mantener la firma original
                   
                    factura.Estado = "Aceptada";

                    // Guardar la factura actualizada en MongoDB
                    await _mongoService.FacturasAceptadas.InsertOneAsync(factura);
                    //guardar en historial de facturas en sql
                    if (factura.FacturaSQLID.HasValue)
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
                    string errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Mensaje"] = $"Error al enviar la factura: {errorContent}";
                }
            }
            catch (Exception Ex)
            {
                //  Capturar errores de conexión HTTP
                TempData["Mensaje"] = $"Error de conexión al enviar a Hacienda: {Ex.Message}";
            }
           
            return RedirectToAction("Index");
        }

        // <summary>
        /// Muestra la vista principal de facturas
        /// 

        public IActionResult Index()
        {
            bool mostrarBoton = false;

            int? facturaId = HttpContext.Session.GetInt32("FacturaSQLID");

            if (facturaId.HasValue)
            {
                var facturaAceptada = _mongoService.FacturasAceptadas
                    .Find(f => f.FacturaSQLID == facturaId && f.Estado == "Aceptada")
                    .FirstOrDefault();

                mostrarBoton = facturaAceptada != null;
            }

            var viewModel = new IndexViewModel
            {
                MostrarBoton = mostrarBoton,
                JsonFactura = TempData["jsonFactura"]?.ToString()
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> GenerarYEnviarPdf()
        {

            var factura = _mongoService.FacturasAceptadas
                .Find(f => f.Estado == "Aceptada")
                .SortByDescending(f => f.Identificacion.FechaHoraEmision)
                .FirstOrDefault();
            if (factura == null)
            {
                TempData["Mensaje"] = "No se encontro una factura aceptada para enviar.";
                return RedirectToAction("Index");
            }

            var FacturaPDF = new ViewAsPdf("FacturaPdf", factura)
            {
                FileName = $"Factura_{factura.FacturaSQLID}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                IsGrayScale = false
            };

            byte[] pdfBytes = await FacturaPDF.BuildFile(ControllerContext);

            bool enviado = await _emailService.SendEmailAsync("biobaudriz123@gmail.com", pdfBytes, $"Factura_{factura.FacturaSQLID}.pdf");

            TempData["Mensaje"] = enviado ? "Factura enviada por correo electrónico." : "Error al enviar la factura por correo electrónico.";
            // Limpiar la sesión para evitar reenvíos
            HttpContext.Session.Remove("FacturaSQLID");
            return RedirectToAction("Index");

        }

    }
}