using System.Diagnostics;
using GestionVentas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionVentas.Context;
using GestionVentas.ViewsModels;
namespace GestionVentas.Controllers

{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbcontext _context;

        public HomeController(ILogger<HomeController> logger, AppDbcontext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var empresaId = HttpContext.Session.GetInt32("EmpresaID");
            var clienteId = HttpContext.Session.GetInt32("ClienteID");
            var nombreUsuario = HttpContext.Session.GetString("NombreEmpleado");

            if(empresaId == null || clienteId == null)
            {
                // Si no hay empresa o cliente seleccionado, redirigir a la selección
                return RedirectToAction("Seleccionar", "Seleccion");
            }

            var empresa = _context.Empresas.FirstOrDefault(e => e.EmpresaID == empresaId);
            var cliente = _context.Clientes.FirstOrDefault(c => c.ClienteID == clienteId);

            var vm = new BienvenidaViewModel
            {
                NombreUsuario = nombreUsuario ?? "Usuario",
                NombreEmpresa = empresa?.Nombre ?? "Empresa no seleccionada",
                NombreCliente = cliente?.Nombre ?? "Cliente no seleccionado"
            };
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
