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

                // SOLO MODIFICAR STOCK SI ES UN MOVIMIENTO MANUAL (NO VIENE DE VENTA)
                // Validar stock disponible para salidas
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
                }
                else if (movimiento.TipoMovimiento == "Entrada")
                {
                    servicio.StockDisponible += movimiento.Cantidad;
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