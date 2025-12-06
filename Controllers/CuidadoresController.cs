// Controllers/CuidadoresController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    [Authorize(Roles = "1")] // Solo Administrador
    public class CuidadoresController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public CuidadoresController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: Cuidadores
        public async Task<IActionResult> Index()
        {
            var cuidadores = await _context.Cuidadores.ToListAsync();
            return View(cuidadores);
        }

        // GET: Cuidadores/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Cuidadores/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Cuidador cuidador)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cuidador);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cuidador registrado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(cuidador);
        }

        // GET: Cuidadores/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuidador = await _context.Cuidadores.FindAsync(id);
            if (cuidador == null)
            {
                return NotFound();
            }
            return View(cuidador);
        }

        // POST: Cuidadores/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Cuidador cuidador)
        {
            if (id != cuidador.IdCuidador)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cuidador);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Cuidador actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CuidadorExists(cuidador.IdCuidador))
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
            return View(cuidador);
        }

        // GET: Cuidadores/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuidador = await _context.Cuidadores
                .FirstOrDefaultAsync(m => m.IdCuidador == id);
            if (cuidador == null)
            {
                return NotFound();
            }

            return View(cuidador);
        }

        // POST: Cuidadores/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var cuidador = await _context.Cuidadores.FindAsync(id);
            if (cuidador != null)
            {
                _context.Cuidadores.Remove(cuidador);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Cuidador eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cuidadores/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cuidador = await _context.Cuidadores
                .FirstOrDefaultAsync(m => m.IdCuidador == id);
            if (cuidador == null)
            {
                return NotFound();
            }

            return View(cuidador);
        }

        private bool CuidadorExists(int id)
        {
            return _context.Cuidadores.Any(e => e.IdCuidador == id);
        }
    }
}