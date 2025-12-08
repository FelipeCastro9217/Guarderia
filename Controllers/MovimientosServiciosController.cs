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

                movimiento.FechaMovimiento = DateTime.Now;

                if (movimiento.TipoMovimiento == "Entrada")
                {
                    // ENTRADA: REDUCE stock (el servicio se está usando), estado "En Proceso"
                    if (servicio.StockDisponible < movimiento.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente. Disponible: {servicio.StockDisponible}";
                        ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
                        ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                        ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
                        return View(movimiento);
                    }

                    servicio.StockDisponible -= movimiento.Cantidad;
                    movimiento.EstadoServicio = "En Proceso";
                    movimiento.FechaCompletado = null;

                    _context.Update(servicio);
                    TempData["Mensaje"] = "Movimiento de Entrada registrado. El stock se ha REDUCIDO (servicio en uso).";
                }
                else if (movimiento.TipoMovimiento == "Salida")
                {
                    // SALIDA: AUMENTA stock (el servicio terminó y vuelve a estar disponible), estado "Completado"
                    servicio.StockDisponible += movimiento.Cantidad;
                    movimiento.EstadoServicio = "Completado";
                    movimiento.FechaCompletado = DateTime.Now;

                    _context.Update(servicio);
                    TempData["Mensaje"] = "Movimiento de Salida registrado. El stock se ha AUMENTADO (servicio disponible nuevamente).";
                }

                _context.Add(movimiento);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Servicios = new SelectList(_context.InventarioServicios, "IdServicio", "NombreServicio");
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Cuidadores = new SelectList(_context.Cuidadores.Where(c => c.Activo), "IdCuidador", "Nombre");
            return View(movimiento);
        }

        // POST: Completar Servicio (Ya no se usa, pero lo dejamos por compatibilidad)
        [HttpPost]
        public async Task<IActionResult> CompletarServicio(int id)
        {
            var movimiento = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);

            if (movimiento == null)
            {
                return Json(new { success = false, message = "Movimiento no encontrado" });
            }

            if (movimiento.EstadoServicio == "Completado")
            {
                return Json(new { success = false, message = "El servicio ya está completado" });
            }

            // Marcar como completado (sin modificar stock ya que se hizo en Crear)
            movimiento.EstadoServicio = "Completado";
            movimiento.FechaCompletado = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Servicio marcado como completado" });
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
                // Revertir el stock según el tipo de movimiento
                if (movimiento.TipoMovimiento == "Entrada")
                {
                    // Si eliminamos una ENTRADA, devolvemos el stock (sumamos)
                    movimiento.Servicio.StockDisponible += movimiento.Cantidad;
                    _context.Update(movimiento.Servicio);
                    TempData["Mensaje"] = $"Movimiento de Entrada eliminado. Se devolvieron {movimiento.Cantidad} unidades al stock.";
                }
                else if (movimiento.TipoMovimiento == "Salida")
                {
                    // Si eliminamos una SALIDA, quitamos el stock (restamos)
                    movimiento.Servicio.StockDisponible -= movimiento.Cantidad;
                    _context.Update(movimiento.Servicio);
                    TempData["Mensaje"] = $"Movimiento de Salida eliminado. Se restaron {movimiento.Cantidad} unidades del stock.";
                }

                _context.MovimientosServicios.Remove(movimiento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}