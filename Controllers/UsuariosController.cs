using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionVentas.Context;
using GestionVentas.Models;
using BCrypt.Net;

namespace GestionVentas.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly Appcontext _context;

        public UsuariosController(Appcontext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = _context.Usuarios.Include(u => u.Roles).Where(u => u.Estado == true);

            return View(await usuarios.ToListAsync());

        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(m => m.UsuarioID == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["RolID"] = new SelectList(_context.Roles, "RolID", "RolID");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombreEmpleado,Usuario,Contra,RolID,Estado")] Usuarios usuarios)
        {
            if (!ModelState.IsValid)
            {
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Campo: {entry.Key}, Error: {error.ErrorMessage}");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                usuarios.Contra = BCrypt.Net.BCrypt.HashPassword(usuarios.Contra);

                _context.Add(usuarios);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RolID"] = new SelectList(_context.Roles, "RolID", "RolID", usuarios.RolID);
            return View(usuarios);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios == null)
            {
                return NotFound();
            }
            ViewData["RolID"] = new SelectList(_context.Roles, "RolID", "RolID", usuarios.RolID);
            return View(usuarios);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("NombreEmpleado,Usuario,Contra,RolID,Estado")] Usuarios usuarios)
        {
            if (id != usuarios.UsuarioID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuarios);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuariosExists(usuarios.UsuarioID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolID"] = new SelectList(_context.Roles, "RolID", "RolID", usuarios.RolID);
            return View(usuarios);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuarios = await _context.Usuarios
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(m => m.UsuarioID == id);
            if (usuarios == null)
            {
                return NotFound();
            }

            return View(usuarios);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuarios = await _context.Usuarios.FindAsync(id);
            if (usuarios != null)
            {
             usuarios.Estado = false; // establecer estado en falso para desactivar al usuario
                _context.Update(usuarios);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuariosExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioID == id);
        }
    }
}
