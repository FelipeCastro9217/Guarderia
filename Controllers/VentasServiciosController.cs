// Controllers/VentasServiciosController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Guarderia.Controllers
{
    public class VentasServiciosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public VentasServiciosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: VentasServicios
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.VentasServicios
                .Include(v => v.Cliente)
                .Include(v => v.Mascota)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Servicio)
                .OrderByDescending(v => v.FechaVenta)
                .ToListAsync();
            return View(ventas);
        }

        // GET: VentasServicios/Crear
        public IActionResult Crear()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Servicios = _context.InventarioServicios.ToList();
            return View();
        }

        // POST: VentasServicios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int IdCliente, int IdMascota, string ServiciosJson)
        {
            try
            {
                if (IdCliente == 0 || IdMascota == 0 || string.IsNullOrEmpty(ServiciosJson))
                {
                    TempData["Error"] = "Debe seleccionar cliente, mascota y al menos un servicio";
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Servicios = _context.InventarioServicios.ToList();
                    return View();
                }

                var servicios = JsonSerializer.Deserialize<List<DetalleServicioTemp>>(ServiciosJson);

                if (servicios == null || !servicios.Any())
                {
                    TempData["Error"] = "Debe agregar al menos un servicio";
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Servicios = _context.InventarioServicios.ToList();
                    return View();
                }

                decimal total = 0;
                var detalles = new List<DetalleVentaServicio>();

                // Validar stock SOLAMENTE (no modificarlo aquí)
                foreach (var servicio in servicios)
                {
                    var servicioDb = await _context.InventarioServicios.FindAsync(servicio.IdServicio);

                    if (servicioDb == null)
                    {
                        TempData["Error"] = $"Servicio con ID {servicio.IdServicio} no encontrado";
                        ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                        ViewBag.Servicios = _context.InventarioServicios.ToList();
                        return View();
                    }

                    if (servicioDb.StockDisponible < servicio.Cantidad)
                    {
                        TempData["Error"] = $"Stock insuficiente para {servicioDb.NombreServicio}. Disponible: {servicioDb.StockDisponible}";
                        ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                        ViewBag.Servicios = _context.InventarioServicios.ToList();
                        return View();
                    }

                    var subtotal = servicio.Cantidad * servicioDb.PrecioUnitario;
                    total += subtotal;

                    detalles.Add(new DetalleVentaServicio
                    {
                        IdServicio = servicio.IdServicio,
                        Cantidad = servicio.Cantidad,
                        PrecioUnitario = servicioDb.PrecioUnitario,
                        Subtotal = subtotal
                    });

                    // NO MODIFICAR STOCK AQUÍ - Solo registrar el movimiento
                    // El stock se modifica SOLO en MovimientosServicios
                }

                // Crear la venta
                var venta = new VentaServicio
                {
                    IdCliente = IdCliente,
                    IdMascota = IdMascota,
                    FechaVenta = DateTime.Now,
                    Total = total,
                    Estado = "Completada",
                    DetallesVenta = detalles
                };

                _context.VentasServicios.Add(venta);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Venta registrada exitosamente. Recuerde registrar el movimiento de servicios correspondiente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar la venta: {ex.Message}";
                ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                ViewBag.Servicios = _context.InventarioServicios.ToList();
                return View();
            }
        }

        // GET: VentasServicios/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.VentasServicios
                .Include(v => v.Cliente)
                .Include(v => v.Mascota)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(m => m.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // Clase auxiliar para deserialización
        private class DetalleServicioTemp
        {
            public int IdServicio { get; set; }
            public int Cantidad { get; set; }
        }
    }
}