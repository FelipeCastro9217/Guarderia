// Controllers/GraficosController.cs
using Guarderia.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    public class GraficosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public GraficosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Datos para gráfico de ventas por mes
            var ventasPorMes = await _context.VentasServicios
                .Where(v => v.FechaVenta.Year == DateTime.Now.Year)
                .GroupBy(v => v.FechaVenta.Month)
                .Select(g => new { Mes = g.Key, Total = g.Sum(v => v.Total), Cantidad = g.Count() })
                .OrderBy(x => x.Mes)
                .ToListAsync();

            ViewBag.VentasPorMes = ventasPorMes;

            // Servicios más vendidos
            var serviciosMasVendidos = await _context.DetallesVentasServicios
                .Include(d => d.Servicio)
                .GroupBy(d => d.Servicio.NombreServicio)
                .Select(g => new { Servicio = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToListAsync();

            ViewBag.ServiciosMasVendidos = serviciosMasVendidos;

            // Ocupación por categoría
            var categorias = await _context.InventarioServicios
                .GroupBy(s => s.Categoria)
                .Select(g => new { Categoria = g.Key, Cantidad = g.Count(), Stock = g.Sum(s => s.StockDisponible) })
                .ToListAsync();

            ViewBag.Categorias = categorias;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerDatosGrafico(string tipoGrafico, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                switch (tipoGrafico)
                {
                    case "ventas":
                        var ventas = await _context.VentasServicios
                            .Where(v => (!fechaInicio.HasValue || v.FechaVenta >= fechaInicio) &&
                                        (!fechaFin.HasValue || v.FechaVenta <= fechaFin))
                            .GroupBy(v => v.FechaVenta.Date)
                            .Select(g => new { Fecha = g.Key.ToString("dd/MM/yyyy"), Total = g.Sum(v => v.Total) })
                            .OrderBy(x => x.Fecha)
                            .ToListAsync();
                        return Json(ventas);

                    case "servicios":
                        var servicios = await _context.DetallesVentasServicios
                            .Include(d => d.Servicio)
                            .GroupBy(d => d.Servicio.NombreServicio)
                            .Select(g => new { Servicio = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
                            .OrderByDescending(x => x.Cantidad)
                            .Take(10)
                            .ToListAsync();
                        return Json(servicios);

                    case "categorias":
                        var categorias = await _context.InventarioServicios
                            .GroupBy(s => s.Categoria)
                            .Select(g => new { Categoria = g.Key, Stock = g.Sum(s => s.StockDisponible) })
                            .ToListAsync();
                        return Json(categorias);

                    default:
                        return Json(new { error = "Tipo de gráfico no válido" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}