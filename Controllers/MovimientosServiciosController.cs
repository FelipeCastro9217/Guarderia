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
                    // ENTRADA: No modifica stock, estado "En Proceso"
                    movimiento.EstadoServicio = "En Proceso";
                    movimiento.FechaCompletado = null;

                    TempData["Mensaje"] = "Movimiento de Entrada registrado. El stock NO se ha modificado. Complete el servicio para generar la salida automática.";
                }
                else if (movimiento.TipoMovimiento == "Salida")
                {
                    // SALIDA: Reduce stock, estado "Pendiente"
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

                    _context.Update(servicio);
                    TempData["Mensaje"] = "Movimiento de Salida registrado. El stock ha sido reducido.";
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

        // POST: Completar Servicio
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

            // Marcar como completado
            movimiento.EstadoServicio = "Completado";
            movimiento.FechaCompletado = DateTime.Now;

            // Si es tipo ENTRADA, crear automáticamente la SALIDA
            if (movimiento.TipoMovimiento == "Entrada")
            {
                // Verificar stock disponible
                var servicio = movimiento.Servicio;
                if (servicio.StockDisponible < movimiento.Cantidad)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Stock insuficiente para generar la salida. Disponible: {servicio.StockDisponible}, Requerido: {movimiento.Cantidad}"
                    });
                }

                // Crear el movimiento de SALIDA automático
                var movimientoSalida = new MovimientoServicio
                {
                    IdServicio = movimiento.IdServicio,
                    IdMascota = movimiento.IdMascota,
                    IdCuidador = movimiento.IdCuidador,
                    TipoMovimiento = "Salida",
                    Cantidad = movimiento.Cantidad,
                    FechaMovimiento = DateTime.Now,
                    EstadoServicio = "Completado",
                    FechaCompletado = DateTime.Now,
                    Observaciones = $"Salida automática generada al completar Entrada #{movimiento.IdMovimiento}"
                };

                // Reducir el stock
                servicio.StockDisponible -= movimiento.Cantidad;

                _context.MovimientosServicios.Add(movimientoSalida);
                _context.Update(servicio);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Entrada completada. Se generó automáticamente la Salida y se redujo el stock en {movimiento.Cantidad} unidades."
                });
            }
            else
            {
                // Es una SALIDA normal, solo marcar como completado
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Servicio de Salida completado exitosamente" });
            }
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
                // SOLO revertir stock si es SALIDA
                if (movimiento.TipoMovimiento == "Salida")
                {
                    movimiento.Servicio.StockDisponible += movimiento.Cantidad;
                    _context.Update(movimiento.Servicio);
                    TempData["Mensaje"] = $"Movimiento de Salida eliminado. Se devolvieron {movimiento.Cantidad} unidades al stock.";
                }
                else
                {
                    // Es ENTRADA, no se modifica stock
                    TempData["Mensaje"] = "Movimiento de Entrada eliminado. El stock NO fue modificado.";
                }

                _context.MovimientosServicios.Remove(movimiento);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}