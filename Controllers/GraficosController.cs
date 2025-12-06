// Controllers/GraficosController.cs
using Guarderia.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    [Authorize(Roles = "1")] // Solo Administrador
    public class GraficosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public GraficosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // Vista principal de gráficos
        public async Task<IActionResult> Index()
        {
            // Cargar datos iniciales
            var servicios = await _context.InventarioServicios.ToListAsync();
            ViewBag.Servicios = servicios;

            // Datos para categorías
            var categorias = await _context.InventarioServicios
                .GroupBy(s => s.Categoria)
                .Select(g => new { Categoria = g.Key, Stock = g.Sum(s => s.StockDisponible) })
                .ToListAsync();
            ViewBag.Categorias = categorias;

            // Datos de ventas por mes (año actual)
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
                .Take(10)
                .ToListAsync();
            ViewBag.ServiciosMasVendidos = serviciosMasVendidos;

            // Cuidadores con más servicios asignados
            var cuidadoresActivos = await _context.MovimientosServicios
                .Include(m => m.Cuidador)
                .Where(m => m.IdCuidador != null && m.TipoMovimiento == "Salida")
                .GroupBy(m => new { m.Cuidador.Nombre, m.Cuidador.Apellido })
                .Select(g => new {
                    NombreCompleto = g.Key.Nombre + " " + g.Key.Apellido,
                    TotalServicios = g.Count()
                })
                .OrderByDescending(x => x.TotalServicios)
                .Take(10)
                .ToListAsync();
            ViewBag.CuidadoresActivos = cuidadoresActivos;

            return View();
        }

        // Generar gráfico por parámetros
        [HttpPost]
        public async Task<JsonResult> GenerarGrafico(string tipoGrafico, DateTime? fechaInicio, DateTime? fechaFin, string? categoria)
        {
            try
            {
                switch (tipoGrafico)
                {
                    case "servicios_stock":
                        var servicios = await _context.InventarioServicios
                            .Where(s => string.IsNullOrEmpty(categoria) || s.Categoria == categoria)
                            .Select(s => new { s.NombreServicio, s.StockDisponible })
                            .ToListAsync();
                        return Json(servicios);

                    case "ventas_periodo":
                        var ventasQuery = _context.VentasServicios.AsQueryable();

                        if (fechaInicio.HasValue)
                            ventasQuery = ventasQuery.Where(v => v.FechaVenta >= fechaInicio.Value);

                        if (fechaFin.HasValue)
                            ventasQuery = ventasQuery.Where(v => v.FechaVenta <= fechaFin.Value);

                        var ventas = await ventasQuery
                            .Select(v => new {
                                Fecha = v.FechaVenta,
                                Total = v.Total
                            })
                            .ToListAsync();

                        var ventasAgrupadas = ventas
                            .GroupBy(v => v.Fecha.Date)
                            .Select(g => new {
                                Fecha = g.Key.ToString("dd/MM/yyyy"),
                                Total = g.Sum(v => v.Total)
                            })
                            .OrderBy(x => x.Fecha)
                            .ToList();

                        return Json(ventasAgrupadas);

                    case "categorias_stock":
                        var categorias = await _context.InventarioServicios
                            .GroupBy(s => s.Categoria)
                            .Select(g => new { Categoria = g.Key, Stock = g.Sum(s => s.StockDisponible) })
                            .ToListAsync();
                        return Json(categorias);

                    case "servicios_vendidos":
                        var serviciosVendidos = await _context.DetallesVentasServicios
                            .Include(d => d.Servicio)
                            .Include(d => d.Venta)
                            .Where(d => (!fechaInicio.HasValue || d.Venta.FechaVenta >= fechaInicio) &&
                                       (!fechaFin.HasValue || d.Venta.FechaVenta <= fechaFin))
                            .GroupBy(d => d.Servicio.NombreServicio)
                            .Select(g => new { Servicio = g.Key, Cantidad = g.Sum(d => d.Cantidad) })
                            .OrderByDescending(x => x.Cantidad)
                            .Take(10)
                            .ToListAsync();
                        return Json(serviciosVendidos);

                    case "cuidadores_servicios":
                        var cuidadores = await _context.MovimientosServicios
                            .Include(m => m.Cuidador)
                            .Where(m => m.IdCuidador != null &&
                                       m.TipoMovimiento == "Salida" &&
                                       (!fechaInicio.HasValue || m.FechaMovimiento >= fechaInicio) &&
                                       (!fechaFin.HasValue || m.FechaMovimiento <= fechaFin))
                            .GroupBy(m => new { m.Cuidador.Nombre, m.Cuidador.Apellido })
                            .Select(g => new {
                                Cuidador = g.Key.Nombre + " " + g.Key.Apellido,
                                TotalServicios = g.Count()
                            })
                            .OrderByDescending(x => x.TotalServicios)
                            .Take(10)
                            .ToListAsync();
                        return Json(cuidadores);

                    case "movimientos_tipo":
                        var movimientos = await _context.MovimientosServicios
                            .Where(m => (!fechaInicio.HasValue || m.FechaMovimiento >= fechaInicio) &&
                                       (!fechaFin.HasValue || m.FechaMovimiento <= fechaFin))
                            .GroupBy(m => m.TipoMovimiento)
                            .Select(g => new { Tipo = g.Key, Cantidad = g.Sum(m => m.Cantidad) })
                            .ToListAsync();
                        return Json(movimientos);

                    case "ventas_mensuales":
                        var ventasMensuales = await _context.VentasServicios
                            .Where(v => v.FechaVenta.Year == DateTime.Now.Year)
                            .GroupBy(v => v.FechaVenta.Month)
                            .Select(g => new { Mes = g.Key, Total = g.Sum(v => v.Total) })
                            .OrderBy(x => x.Mes)
                            .ToListAsync();
                        return Json(ventasMensuales);

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