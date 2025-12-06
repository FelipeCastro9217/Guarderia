// Controllers/ReportesController.cs
using Guarderia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    public class ReportesController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public ReportesController(GuarderiaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerarReporte(string tipoReporte, DateTime? fechaInicio, DateTime? fechaFin, int? idCliente)
        {
            ViewBag.TipoReporte = tipoReporte;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            switch (tipoReporte)
            {
                case "ventas":
                    var ventas = await _context.VentasServicios
                        .Include(v => v.Cliente)
                        .Include(v => v.Mascota)
                        .Include(v => v.DetallesVenta)
                            .ThenInclude(d => d.Servicio)
                        .Where(v => (!fechaInicio.HasValue || v.FechaVenta >= fechaInicio) &&
                                    (!fechaFin.HasValue || v.FechaVenta <= fechaFin))
                        .OrderByDescending(v => v.FechaVenta)
                        .ToListAsync();

                    ViewBag.TotalVentas = ventas.Sum(v => v.Total);
                    ViewBag.CantidadVentas = ventas.Count;
                    return View("ReporteVentas", ventas);

                case "servicios":
                    var servicios = await _context.InventarioServicios.ToListAsync();
                    return View("ReporteServicios", servicios);

                case "clientes":
                    var clientes = await _context.Clientes
                        .Include(c => c)
                        .ToListAsync();
                    return View("ReporteClientes", clientes);

                case "movimientos":
                    var movimientos = await _context.MovimientosServicios
                        .Include(m => m.Servicio)
                        .Include(m => m.Mascota)
                            .ThenInclude(ma => ma.Cliente)
                        .Where(m => (!fechaInicio.HasValue || m.FechaMovimiento >= fechaInicio) &&
                                    (!fechaFin.HasValue || m.FechaMovimiento <= fechaFin))
                        .OrderByDescending(m => m.FechaMovimiento)
                        .ToListAsync();
                    return View("ReporteMovimientos", movimientos);

                default:
                    return View("Index");
            }
        }
    }
}

