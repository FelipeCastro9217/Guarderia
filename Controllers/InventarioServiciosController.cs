// Controllers/InventarioServiciosController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    public class InventarioServiciosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public InventarioServiciosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: InventarioServicios
        public async Task<IActionResult> Index()
        {
            var servicios = await _context.InventarioServicios.ToListAsync();
            return View(servicios);
        }

        // GET: InventarioServicios/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: InventarioServicios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(InventarioServicio servicio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(servicio);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Servicio creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            return View(servicio);
        }

        // GET: InventarioServicios/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.InventarioServicios.FindAsync(id);
            if (servicio == null)
            {
                return NotFound();
            }
            return View(servicio);
        }

        // POST: InventarioServicios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, InventarioServicio servicio)
        {
            if (id != servicio.IdServicio)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicio);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Servicio actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioExists(servicio.IdServicio))
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
            return View(servicio);
        }

        // GET: InventarioServicios/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.InventarioServicios
                .FirstOrDefaultAsync(m => m.IdServicio == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        // POST: InventarioServicios/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var servicio = await _context.InventarioServicios.FindAsync(id);
            if (servicio != null)
            {
                // Verificar si tiene movimientos o ventas asociadas
                var tieneMovimientos = await _context.MovimientosServicios.AnyAsync(m => m.IdServicio == id);
                var tieneVentas = await _context.DetallesVentasServicios.AnyAsync(d => d.IdServicio == id);

                if (tieneMovimientos || tieneVentas)
                {
                    TempData["Error"] = "No se puede eliminar el servicio porque tiene movimientos o ventas asociadas";
                    return RedirectToAction(nameof(Index));
                }

                _context.InventarioServicios.Remove(servicio);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Servicio eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: InventarioServicios/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var servicio = await _context.InventarioServicios
                .FirstOrDefaultAsync(m => m.IdServicio == id);
            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }

        private bool ServicioExists(int id)
        {
            return _context.InventarioServicios.Any(e => e.IdServicio == id);
        }
    }
}