using Microsoft.AspNetCore.Mvc;
using GestionVentas.Models_Mongo;
namespace GestionVentas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HaciendaController : ControllerBase
    {
        [HttpPost("EnviarFactura")]
        public IActionResult EnviarFactura([FromBody] FacturaJson factura)
        {
            if (factura == null || string.IsNullOrWhiteSpace(factura.Firma))
            {
                return BadRequest(new
                {
                    estado = "Rechazada",
                    motivo = "Factura inválida o sin firma digital."
                });


            }

            //validacions basicas(simulando la validacion de hacienda)
            if (string.IsNullOrWhiteSpace(factura.Emisor) ||
               string.IsNullOrWhiteSpace(factura.Cliente) ||
               factura.FechaEmision == default ||
               factura.Total <= 0 ||
               factura.Detalles == null ||
               factura.Detalles.Count == 0)
            {
                return BadRequest(new
                {
                    estado = "Rechazada",
                    motivo = "Factura incompleta o inválida."
                });
            }

            return Ok(new
            {
                estado = "Aceptada",
                fechaRecepcion = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                numControl = Guid.NewGuid().ToString()

            });
        }
    }
}
