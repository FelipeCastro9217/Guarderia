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

                // Validar stock SOLAMENTE (no modificarlo)
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

                    // NO validamos stock porque la factura es el pago de un servicio ya completado
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

                // NUEVO: Generar automáticamente movimiento de SALIDA para cada servicio
                foreach (var detalle in detalles)
                {
                    var servicioDb = await _context.InventarioServicios.FindAsync(detalle.IdServicio);

                    if (servicioDb != null)
                    {
                        // Buscar el movimiento de ENTRADA asociado (más reciente para esta mascota y servicio)
                        var movimientoEntrada = await _context.MovimientosServicios
                            .Where(m => m.IdServicio == detalle.IdServicio
                                     && m.IdMascota == IdMascota
                                     && m.TipoMovimiento == "Entrada"
                                     && m.EstadoServicio == "En Proceso")
                            .OrderByDescending(m => m.FechaMovimiento)
                            .FirstOrDefaultAsync();

                        int? idCuidadorAsignado = null;

                        // Si existe una ENTRADA en proceso, completarla y obtener su cuidador
                        if (movimientoEntrada != null)
                        {
                            movimientoEntrada.EstadoServicio = "Completado";
                            movimientoEntrada.FechaCompletado = DateTime.Now;
                            idCuidadorAsignado = movimientoEntrada.IdCuidador;
                            _context.Update(movimientoEntrada);
                        }

                        // Crear movimiento de SALIDA (aumenta el stock) con el mismo cuidador
                        var movimientoSalida = new MovimientoServicio
                        {
                            IdServicio = detalle.IdServicio,
                            IdMascota = IdMascota,
                            IdCuidador = idCuidadorAsignado, // Mantener el cuidador de la Entrada
                            TipoMovimiento = "Salida",
                            Cantidad = detalle.Cantidad,
                            FechaMovimiento = DateTime.Now,
                            EstadoServicio = "Completado",
                            FechaCompletado = DateTime.Now,
                            Observaciones = $"Salida automática por Factura {numeroFactura} - Servicio pagado y completado"
                        };

                        // AUMENTAR el stock (el servicio terminó y vuelve a estar disponible)
                        servicioDb.StockDisponible += detalle.Cantidad;

                        _context.MovimientosServicios.Add(movimientoSalida);
                        _context.Update(servicioDb);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Factura {numeroFactura} generada exitosamente. Se completaron las Entradas pendientes, se generaron los movimientos de Salida automáticos y se actualizó el stock.";
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
                // Buscar los movimientos de Salida generados por esta factura
                var movimientosSalida = await _context.MovimientosServicios
                    .Where(m => m.Observaciones.Contains($"Factura {factura.NumeroFactura}"))
                    .Include(m => m.Servicio)
                    .ToListAsync();

                // Revertir los movimientos de Salida (restar stock)
                foreach (var movimiento in movimientosSalida)
                {
                    if (movimiento.Servicio != null)
                    {
                        movimiento.Servicio.StockDisponible -= movimiento.Cantidad;
                        _context.Update(movimiento.Servicio);
                    }
                    _context.MovimientosServicios.Remove(movimiento);
                }

                factura.Estado = "Anulada";
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"Factura {factura.NumeroFactura} anulada exitosamente. Se revirtieron los movimientos de Salida y el stock fue actualizado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}