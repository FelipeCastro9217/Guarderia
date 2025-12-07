// Controllers/PanelClienteController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Guarderia.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class PanelClienteController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public PanelClienteController(GuarderiaDbContext context)
        {
            _context = context;
        }

        private int ObtenerIdCliente()
        {
            var idCliente = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idCliente);
        }

        // GET: Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var idCliente = ObtenerIdCliente();

            var cliente = await _context.Clientes
                .Include(c => c.Mascotas)
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (cliente == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Estadísticas
            ViewBag.CantidadMascotas = cliente.Mascotas?.Count ?? 0;
            ViewBag.AgendamientosPendientes = await _context.Agendamientos
                .CountAsync(a => a.IdCliente == idCliente && a.Estado == "Pendiente");
            ViewBag.ServiciosCompletados = await _context.MovimientosServicios
                .CountAsync(m => m.Mascota.IdCliente == idCliente && m.EstadoServicio == "Completado");

            return View(cliente);
        }

        // GET: MisMascotas
        public async Task<IActionResult> MisMascotas()
        {
            var idCliente = ObtenerIdCliente();
            var mascotas = await _context.Mascotas
                .Where(m => m.IdCliente == idCliente)
                .ToListAsync();

            return View(mascotas);
        }

        // GET: RegistrarMascota
        public IActionResult RegistrarMascota()
        {
            return View();
        }

        // POST: RegistrarMascota
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMascota(Mascota mascota)
        {
            var idCliente = ObtenerIdCliente();
            mascota.IdCliente = idCliente;

            if (ModelState.IsValid)
            {
                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Mascota registrada exitosamente";
                return RedirectToAction(nameof(MisMascotas));
            }

            return View(mascota);
        }

        // GET: EditarMascota/5
        public async Task<IActionResult> EditarMascota(int? id)
        {
            if (id == null) return NotFound();

            var idCliente = ObtenerIdCliente();
            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.IdMascota == id && m.IdCliente == idCliente);

            if (mascota == null) return NotFound();

            return View(mascota);
        }

        // POST: EditarMascota/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarMascota(int id, Mascota mascota)
        {
            if (id != mascota.IdMascota) return NotFound();

            var idCliente = ObtenerIdCliente();
            mascota.IdCliente = idCliente;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mascota);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Mascota actualizada exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Mascotas.AnyAsync(m => m.IdMascota == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(MisMascotas));
            }

            return View(mascota);
        }

        // GET: AgendarServicio
        public async Task<IActionResult> AgendarServicio()
        {
            var idCliente = ObtenerIdCliente();
            var mascotas = await _context.Mascotas
                .Where(m => m.IdCliente == idCliente)
                .ToListAsync();

            if (!mascotas.Any())
            {
                TempData["Error"] = "Debes registrar al menos una mascota antes de agendar servicios";
                return RedirectToAction(nameof(RegistrarMascota));
            }

            ViewBag.Mascotas = new SelectList(mascotas, "IdMascota", "Nombre");
            ViewBag.Servicios = await _context.InventarioServicios
                .Where(s => s.StockDisponible > 0)
                .ToListAsync();

            return View();
        }

        // POST: AgendarServicio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarServicio(Agendamiento agendamiento)
        {
            var idCliente = ObtenerIdCliente();
            agendamiento.IdCliente = idCliente;
            agendamiento.FechaSolicitud = DateTime.Now;
            agendamiento.Estado = "Pendiente";

            // Validar que la fecha sea futura
            if (agendamiento.FechaSolicitada.Date < DateTime.Now.Date)
            {
                TempData["Error"] = "La fecha debe ser igual o posterior a hoy";
                await CargarDatosAgendamiento();
                return View(agendamiento);
            }

            // Validar que la mascota pertenezca al cliente
            var mascotaValida = await _context.Mascotas
                .AnyAsync(m => m.IdMascota == agendamiento.IdMascota && m.IdCliente == idCliente);

            if (!mascotaValida)
            {
                TempData["Error"] = "Mascota no válida";
                await CargarDatosAgendamiento();
                return View(agendamiento);
            }

            if (ModelState.IsValid)
            {
                _context.Agendamientos.Add(agendamiento);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Servicio agendado exitosamente. Nos contactaremos contigo para confirmar.";
                return RedirectToAction(nameof(MisAgendamientos));
            }

            await CargarDatosAgendamiento();
            return View(agendamiento);
        }

        private async Task CargarDatosAgendamiento()
        {
            var idCliente = ObtenerIdCliente();
            var mascotas = await _context.Mascotas
                .Where(m => m.IdCliente == idCliente)
                .ToListAsync();

            ViewBag.Mascotas = new SelectList(mascotas, "IdMascota", "Nombre");
            ViewBag.Servicios = await _context.InventarioServicios
                .Where(s => s.StockDisponible > 0)
                .ToListAsync();
        }

        // GET: MisAgendamientos
        public async Task<IActionResult> MisAgendamientos()
        {
            var idCliente = ObtenerIdCliente();
            var agendamientos = await _context.Agendamientos
                .Include(a => a.Mascota)
                .Include(a => a.Servicio)
                .Where(a => a.IdCliente == idCliente)
                .OrderByDescending(a => a.FechaSolicitud)
                .ToListAsync();

            return View(agendamientos);
        }

        // POST: CancelarAgendamiento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarAgendamiento(int id)
        {
            var idCliente = ObtenerIdCliente();
            var agendamiento = await _context.Agendamientos
                .FirstOrDefaultAsync(a => a.IdAgendamiento == id && a.IdCliente == idCliente);

            if (agendamiento == null)
            {
                TempData["Error"] = "Agendamiento no encontrado";
                return RedirectToAction(nameof(MisAgendamientos));
            }

            if (agendamiento.Estado == "Completado")
            {
                TempData["Error"] = "No puedes cancelar un servicio completado";
                return RedirectToAction(nameof(MisAgendamientos));
            }

            agendamiento.Estado = "Cancelado";
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Agendamiento cancelado exitosamente";
            return RedirectToAction(nameof(MisAgendamientos));
        }

        // GET: HistorialServicios
        public async Task<IActionResult> HistorialServicios()
        {
            var idCliente = ObtenerIdCliente();

            var movimientos = await _context.MovimientosServicios
                .Include(m => m.Servicio)
                .Include(m => m.Mascota)
                .Include(m => m.Cuidador)
                .Where(m => m.Mascota.IdCliente == idCliente)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();

            return View(movimientos);
        }

        // GET: MisFacturas
        public async Task<IActionResult> MisFacturas()
        {
            var idCliente = ObtenerIdCliente();

            var facturas = await _context.Facturas
                .Include(f => f.Mascota)
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .Where(f => f.IdCliente == idCliente)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            return View(facturas);
        }

        // GET: DetalleFactura/5
        public async Task<IActionResult> DetalleFactura(int? id)
        {
            if (id == null) return NotFound();

            var idCliente = ObtenerIdCliente();
            var factura = await _context.Facturas
                .Include(f => f.Mascota)
                .Include(f => f.DetallesFactura)
                    .ThenInclude(d => d.Servicio)
                .FirstOrDefaultAsync(f => f.IdFactura == id && f.IdCliente == idCliente);

            if (factura == null) return NotFound();

            return View(factura);
        }

        // GET: MiPerfil
        public async Task<IActionResult> MiPerfil()
        {
            var idCliente = ObtenerIdCliente();
            var cliente = await _context.Clientes.FindAsync(idCliente);

            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: ActualizarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(Cliente cliente)
        {
            var idCliente = ObtenerIdCliente();

            if (idCliente != cliente.IdCliente)
            {
                return Forbid();
            }

            // Preservar campos que no se deben modificar
            var clienteExistente = await _context.Clientes.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCliente == idCliente);

            if (clienteExistente == null) return NotFound();

            cliente.FechaRegistro = clienteExistente.FechaRegistro;
            cliente.CuentaActiva = clienteExistente.CuentaActiva;

            // Si no cambió la clave, mantener la anterior
            if (string.IsNullOrEmpty(cliente.Clave))
            {
                cliente.Clave = clienteExistente.Clave;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Perfil actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Clientes.AnyAsync(c => c.IdCliente == idCliente))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(MiPerfil));
            }

            return View("MiPerfil", cliente);
        }
    }
}