using GestionVentas.Context;
using GestionVentas.ViewsModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionVentas.Controllers
{
    public class SeleccionController : Controller
    {

        private readonly AppDbcontext _context;
        public SeleccionController(AppDbcontext context)
        {
            _context = context;
        }

        //GET: /Seleccion de empresa y cliente

        public IActionResult Seleccionar()
        {
            var empresas = _context.Empresas
                .Select(e => new SelectListItem { Value = e.EmpresaID.ToString(), Text = e.Nombre })
                .ToList();

            var viewModel = new SeleccionViewModel
            {
                Empresas = empresas 
            };

            return View(viewModel);

        }

        [HttpGet]
        public JsonResult ObtenerClientesPorEmpresa(int empresaId)
        {
            Console.WriteLine($"Recibiendo empresaId: {empresaId}");
            var clientes = _context.Clientes
                .Where(c => c.EmpresaID == empresaId)
                .Select(c => new 
                {
                    c.ClienteID, 
                    c.Nombre
                }).ToList();

            return Json(clientes);
        }

        [HttpPost]
        public IActionResult GuardarSeleccion(SeleccionViewModel model)
        {
            if (model.EmpresaID == 0 || model.ClienteID == 0)
            {
                TempData["Error"] = "Debe seleccionar una empresa y un cliente.";
                return RedirectToAction("Seleccionar");
            }

            // Guardar la selección en la sesión
            HttpContext.Session.SetInt32("EmpresaID", model.EmpresaID);
            HttpContext.Session.SetInt32("ClienteID", model.ClienteID);
            // Redirigir a la página principal 
            return RedirectToAction("Index", "Home");

        }
    }

}
