using GestionVentas.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GestionVentas.Context;
using GestionVentas.ViewsModels;

namespace GestionVentas.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbcontext _context;

        public LoginController(AppDbcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string ip = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if(Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                ip = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            string userAgent = Request.Headers["User-Agent"].ToString();
            // Registrar el intento de inicio de sesión
            var usuario = await _context.Usuarios
                .Include(u => u.Roles)
                    .ThenInclude(r => r.permisoRol)
                        .ThenInclude(pr => pr.Permiso)
                .FirstOrDefaultAsync(u => u.Usuario == model.Usuario);

            string motivo = "";
            bool loginExitoso = false;

            if (usuario == null)
            {
                motivo = "Usuario no encontrado";
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }
            else if (!BCrypt.Net.BCrypt.Verify(model.Password, usuario.Contra))
            {
                motivo = "Contraseña incorrecta";
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }
            else if (!usuario.Estado)
            {
                motivo = "Usuario inactivo";
                ModelState.AddModelError("", "Usuario inactivo.");
            }
            else
            {
                loginExitoso = true;
                motivo = "Inicio de sesión exitoso";
            }

            //Registrar el intento de inicio de sesión
            _context.LoginIntentos.Add(new LoginIntento
            {
                UsuarioIntentado = model.Usuario,
                IP = ip,
                Navegador = userAgent,
                Fecha = DateTime.Now,
                Exitoso = loginExitoso,
                Motivo = motivo
            });
            await _context.SaveChangesAsync();
            if (!loginExitoso)
            {
                return View(model);
            }

            // Guardar información del usuario en la sesión
            HttpContext.Session.SetInt32("UsuarioID", usuario.UsuarioID);
            HttpContext.Session.SetString("NombreEmpleado", usuario.NombreEmpleado);
            HttpContext.Session.SetString("Rol", usuario.Roles.NombreRol);

            //convertir los permisos a una lista de strings
            var permisos = usuario.Roles.permisoRol
                .Select(pr => pr.Permiso.Descripcion)
                .ToList();

            // Guardar los permisos en la sesión
            HttpContext.Session.SetString("Permisos", string.Join(",", permisos));
            // Redirigir a la página principal o a la página de inicio
            return RedirectToAction("Seleccionar", "Seleccion");

        }
        public IActionResult Logout()
        {
            // Limpiar la sesión
            HttpContext.Session.Clear();
            // Redirigir a la página de inicio de sesión
            return RedirectToAction("Login", "Login");
        }
    }
}
