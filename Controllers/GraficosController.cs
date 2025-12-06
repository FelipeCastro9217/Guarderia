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

        // Vista principal de gráficos con los servicios
        public async Task<IActionResult> Index()
        {
            var servicios = await _context.InventarioServicios.ToListAsync();
            return View(servicios);
        }
    }
}