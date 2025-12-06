// Controllers/FacturasController.cs
using Guarderia.Data;
using Guarderia.Models;
using Guarderia.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Guarderia.Controllers
{
    [Authorize]
    public class FacturasController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public FacturasController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            var facturas = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Mascota)
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();
            return View(facturas);
        }

        // GET: Facturas/Crear
        [Authorize(Roles = "1")] // Solo Administrador
        public IActionResult Crear()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Servicios = _context.InventarioServicios.ToList();
            return View();
        }

        // POST: Facturas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Crear(int IdCliente, int IdMascota, decimal DescuentoGlobal, string? Observaciones, string ServiciosJson)
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

                var servicios = JsonSerializer.Deserialize<List<DetalleFacturaTemp>>(ServiciosJson);

                if (servicios == null || !servicios.Any())
                {
                    TempData["Error"] = "Debe agregar al menos un servicio";
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Servicios = _context.InventarioServicios.ToList();
                    return View();
                }

                decimal subtotal = 0;
                decimal totalDescuentoItems = 0;
                var detalles = new List<DetalleFactura>();

                // Validar stock y crear detalles
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

                    var subtotalItem = servicio.Cantidad * servicioDb.PrecioUnitario;
                    var descuentoItem = servicio.DescuentoItem;
                    var subtotalConDescuento = subtotalItem - descuentoItem;

                    subtotal += subtotalConDescuento;
                    totalDescuentoItems += descuentoItem;

                    detalles.Add(new DetalleFactura
                    {
                        IdServicio = servicio.IdServicio,
                        Cantidad = servicio.Cantidad,
                        PrecioUnitario = servicioDb.PrecioUnitario,
                        DescuentoItem = descuentoItem,
                        Subtotal = subtotalConDescuento
                    });

                    // Actualizar stock
                    servicioDb.StockDisponible -= servicio.Cantidad;

                    // Crear movimiento de salida
                    var movimiento = new MovimientoServicio
                    {
                        IdServicio = servicio.IdServicio,
                        IdMascota = IdMascota,
                        TipoMovimiento = "Salida",
                        Cantidad = servicio.Cantidad,
                        FechaMovimiento = DateTime.Now,
                        Observaciones = "Facturación de servicio"
                    };
                    _context.MovimientosServicios.Add(movimiento);
                }

                // Calcular IVA y total
                decimal iva = subtotal * 0.19m;
                decimal totalDescuento = totalDescuentoItems + DescuentoGlobal;
                decimal total = subtotal + iva - DescuentoGlobal;

                // Generar número de factura
                var ultimaFactura = await _context.Facturas.OrderByDescending(f => f.IdFactura).FirstOrDefaultAsync();
                int numeroConsecutivo = (ultimaFactura?.IdFactura ?? 0) + 1;
                string numeroFactura = $"FA-{DateTime.Now:yyyyMMdd}-{numeroConsecutivo:D4}";

                // Crear la factura
                var factura = new Factura
                {
                    NumeroFactura = numeroFactura,
                    IdCliente = IdCliente,
                    IdMascota = IdMascota,
                    FechaEmision = DateTime.Now,
                    Subtotal = subtotal,
                    Iva = iva,
                    Descuento = totalDescuento,
                    Total = total,
                    Estado = "Pagada",
                    Observaciones = Observaciones,
                    DetallesFactura = detalles
                };

                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Factura {numeroFactura} generada exitosamente";
                return RedirectToAction(nameof(Detalles), new { id = factura.IdFactura });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar la factura: {ex.Message}";
                ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                ViewBag.Servicios = _context.InventarioServicios.ToList();
                return View();
            }
        }

        // GET: Facturas/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Mascota)
                    .ThenInclude(m => m.Cliente)
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(m => m.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Facturas/Anular/5
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Anular(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Mascota)
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(m => m.IdFactura == id);

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // POST: Facturas/Anular/5
        [HttpPost, ActionName("Anular")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AnularConfirmado(int id)
        {
            var factura = await _context.Facturas
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(f => f.IdFactura == id);

            if (factura != null && factura.Estado != "Anulada")
            {
                // Revertir stock
                foreach (var detalle in factura.DetallesFactura)
                {
                    if (detalle.Servicio != null)
                    {
                        detalle.Servicio.StockDisponible += detalle.Cantidad;
                    }

                    // Crear movimiento de entrada
                    var movimiento = new MovimientoServicio
                    {
                        IdServicio = detalle.IdServicio,
                        IdMascota = factura.IdMascota,
                        TipoMovimiento = "Entrada",
                        Cantidad = detalle.Cantidad,
                        FechaMovimiento = DateTime.Now,
                        Observaciones = $"Anulación de factura {factura.NumeroFactura}"
                    };
                    _context.MovimientosServicios.Add(movimiento);
                }

                factura.Estado = "Anulada";
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"Factura {factura.NumeroFactura} anulada exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
