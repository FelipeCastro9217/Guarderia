// Controllers/MovimientosServiciosController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    public class MovimientosServiciosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public MovimientosServiciosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: MovimientosServicios
        public async Task<IActionResult> Index()
        {
            var movimientos = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .Include(m => m.Mascota)
                    .ThenInclude(ma => ma.Cliente)
                .Include(m => m.Cuidador)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
            return View(movimientos);
        }

        // GET: MovimientosServicios/Crear
        public IActionResult Crear()
        {
            ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
            return View();
        }

        // POST: MovimientosServicios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(MovimientoServicio movimiento)
        {
            if (ModelState.IsValid)
            {
                var servicio = await _context.InventarioServicios.FindAsync(movimiento.IdServicio);

                if (servicio == null)
                {
                    TempData["Error"] = "El servicio seleccionado no existe";
                    ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
                    return View(movimiento);
                }

                // SOLO MODIFICAR STOCK SI ES SALIDA (cuando el servicio se usa realmente)
                if (movimiento.TipoMovimiento == "Salida")
                {
                    if (servicio.StockDisponible < movimiento.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente. Disponible: {servicio.StockDisponible}";
                        ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
                        ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                        ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
                        return View(movimiento);
                    }
                    servicio.StockDisponible -= movimiento.Cantidad;
                    movimiento.EstadoServicio = "Pendiente";
                }
                else if (movimiento.TipoMovimiento == "Entrada")
                {
                    // Solo aumentar stock en casos de entrada (devoluciones, compras)
                    servicio.StockDisponible += movimiento.Cantidad;
                    movimiento.EstadoServicio = "Completado";
                    movimiento.FechaCompletado = DateTime.Now;
                }

                movimiento.FechaMovimiento = DateTime.Now;
                _context.Add(movimiento);
                _context.Update(servicio);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Movimiento registrado exitosamente";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
            return View(movimiento);
        }

        // POST: Completar Servicio
        [HttpPost]
        public async Task<IActionResult> CompletarServicio(int id)
        {
            var movimiento = await _context.MovimientosServicios.FindAsync(id);

            if (movimiento == null)
            {
                return Json(new { success = false, message = "Movimiento no encontrado" });
            }

            if (movimiento.TipoMovimiento != "Salida")
            {
                return Json(new { success = false, message = "Solo se pueden completar servicios de tipo Salida" });
            }

            if (movimiento.EstadoServicio == "Completado")
            {
                return Json(new { success = false, message = "El servicio ya está completado" });
            }

            movimiento.EstadoServicio = "Completado";
            movimiento.FechaCompletado = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Servicio completado exitosamente" });
        }

        // GET: AsignarCuidador
        public async Task<IActionResult> AsignarCuidador(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .Include(m => m.Mascota)
                    .ThenInclude(ma => ma.Cliente)
                .Include(m => m.Cuidador)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
            return View(movimiento);
        }

        // POST: AsignarCuidador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarCuidador(int IdMovimiento, int IdCuidador)
        {
            var movimiento = await _context.MovimientosServicios.FindAsync(IdMovimiento);

            if (movimiento == null)
            {
                TempData["Error"] = "Movimiento no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var cuidador = await _context.Cuidadores.FindAsync(IdCuidador);

            if (cuidador == null || !cuidador.Activo)
            {
                TempData["Error"] = "Cuidador no válido o inactivo";
                return RedirectToAction(nameof(AsignarCuidador), new { id = IdMovimiento });
            }

            movimiento.IdCuidador = IdCuidador;

            if (movimiento.EstadoServicio == "Pendiente")
            {
                movimiento.EstadoServicio = "En Proceso";
            }

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Cuidador {cuidador.Nombre} {cuidador.Apellido} asignado exitosamente";
            return RedirectToAction(nameof(Index));
        }

        // GET: MovimientosServicios/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .Include(m => m.Mascota)
                    .ThenInclude(ma => ma.Cliente)
                .Include(m => m.Cuidador)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // GET: MovimientosServicios/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movimiento = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .Include(m => m.Mascota)
                    .ThenInclude(ma => ma.Cliente)
                .Include(m => m.Cuidador)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return NotFound();
            }

            return View(movimiento);
        }

        // POST: MovimientosServicios/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var movimiento = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento != null)
            {
                // Revertir el movimiento en el inventario
                if (movimiento.TipoMovimiento == "Salida")
                {
                    movimiento.Servicio.StockDisponible += movimiento.Cantidad;
                }
                else
                {
                    movimiento.Servicio.StockDisponible -= movimiento.Cantidad;
                }

                _context.MovimientosServicios.Remove(movimiento);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Movimiento eliminado y stock actualizado";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}