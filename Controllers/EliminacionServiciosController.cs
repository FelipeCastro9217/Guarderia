// Controllers/EliminacionServiciosController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Guarderia.Controllers
{
    public class EliminacionServiciosController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public EliminacionServiciosController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: EliminacionServicios
        public IActionResult Index()
        {
            ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
            ViewBag.Servicios = _context.InventarioServicios.ToList();
            return View();
        }

        // POST: EliminacionServicios/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int IdCliente, int IdMascota, string ServiciosJson)
        {
            try
            {
                if (IdCliente == 0 || IdMascota == 0 || string.IsNullOrEmpty(ServiciosJson))
                {
                    TempData["Error"] = "Debe seleccionar cliente, mascota y al menos un servicio";
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Servicios = _context.InventarioServicios.ToList();
                    return View("Index");
                }

                var servicios = JsonSerializer.Deserialize<List<ServicioEliminacionTemp>>(ServiciosJson);

                if (servicios == null || !servicios.Any())
                {
                    TempData["Error"] = "Debe agregar al menos un servicio";
                    ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                    ViewBag.Servicios = _context.InventarioServicios.ToList();
                    return View("Index");
                }

                // Procesar cada servicio
                foreach (var servicio in servicios)
                {
                    var servicioDb = await _context.InventarioServicios.FindAsync(servicio.IdServicio);

                    if (servicioDb == null)
                    {
                        TempData["Error"] = $"Servicio con ID {servicio.IdServicio} no encontrado";
                        ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                        ViewBag.Servicios = _context.InventarioServicios.ToList();
                        return View("Index");
                    }

                    // Aumentar el stock (devolver servicios)
                    servicioDb.StockDisponible += servicio.Cantidad;

                    // Crear movimiento de entrada
                    var movimiento = new MovimientoServicio
                    {
                        IdServicio = servicio.IdServicio,
                        IdMascota = IdMascota,
                        TipoMovimiento = "Entrada",
                        Cantidad = servicio.Cantidad,
                        FechaMovimiento = DateTime.Now,
                        Observaciones = "Eliminación/Devolución de servicio"
                    };
                    _context.MovimientosServicios.Add(movimiento);
                }

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Servicios eliminados y stock actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar la eliminación: {ex.Message}";
                ViewBag.Clientes = new SelectList(_context.Clientes, "IdCliente", "Nombre");
                ViewBag.Servicios = _context.InventarioServicios.ToList();
                return View("Index");
            }
        }

        // Clase auxiliar para deserialización
        private class ServicioEliminacionTemp
        {
            public int IdServicio { get; set; }
            public int Cantidad { get; set; }
        }
    }
}