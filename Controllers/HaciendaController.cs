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
            if (factura.Identificacion == null||
               factura.Emisor == null ||
               factura.Receptor == null ||
               factura.CuerpoDocumento == null || factura.CuerpoDocumento.Count == 0 ||
               factura.Resumen == null
               )
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
