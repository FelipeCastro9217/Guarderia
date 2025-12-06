// Controllers/MascotasController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    public class MascotasController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public MascotasController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: Mascotas
        public async Task<IActionResult> Index()
        {
            var mascotas = await _context.Mascotas
                .Include(m => m.Cliente)
                .ToListAsync();
            return View(mascotas);
        }

        // GET: Mascotas/Crear
        public IActionResult Crear()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            return View();
        }

        // POST: Mascotas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Mascota mascota)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mascota);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Mascota registrada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre", mascota.IdCliente);
            return View(mascota);
        }

        // GET: Mascotas/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null)
            {
                return NotFound();
            }

            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre", mascota.IdCliente);
            return View(mascota);
        }

        // POST: Mascotas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mascota);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Mascota actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MascotaExists(mascota.IdMascota))
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
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre", mascota.IdCliente);
            return View(mascota);
        }

        // GET: Mascotas/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.IdMascota == id);
            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        // POST: Mascotas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota != null)
            {
                // Verificar si tiene movimientos o ventas asociadas
                var tieneMovimientos = await _context.MovimientosServicios.AnyAsync(m => m.IdMascota == id);
                var tieneVentas = await _context.VentasServicios.AnyAsync(v => v.IdMascota == id);

                if (tieneMovimientos || tieneVentas)
                {
                    TempData["Error"] = "No se puede eliminar la mascota porque tiene movimientos o ventas asociadas";
                    return RedirectToAction(nameof(Index));
                }

                _context.Mascotas.Remove(mascota);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Mascota eliminada exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Mascotas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mascota = await _context.Mascotas
                .Include(m => m.Cliente)
                .FirstOrDefaultAsync(m => m.IdMascota == id);
            if (mascota == null)
            {
                return NotFound();
            }

            return View(mascota);
        }

        // API para obtener mascotas por cliente (usado en ventas)
        [HttpGet]
        public async Task<JsonResult> ObtenerMascotasPorCliente(int idCliente)
        {
            var mascotas = await _context.Mascotas
                .Where(m => m.IdCliente == idCliente)
                .Select(m => new { m.IdMascota, m.Nombre })
                .ToListAsync();
            return Json(mascotas);
        }

        private bool MascotaExists(int id)
        {
            return _context.Mascotas.Any(e => e.IdMascota == id);
        }
    }
}