// Controllers/PerfilUsuarioController.cs
using Guarderia.Data;
using Guarderia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Guarderia.Controllers
{
    [Authorize(Roles = "1,2")] // Administrador y Empleado
    public class PerfilUsuarioController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public PerfilUsuarioController(GuarderiaDbContext context)
        {
            _context = context;
        }

        private int ObtenerIdUsuario()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == email);
            return usuario?.IdUsuario ?? 0;
        }

        // GET: PerfilUsuario/MiPerfil
        public async Task<IActionResult> MiPerfil()
        {
            var idUsuario = ObtenerIdUsuario();

            if (idUsuario == 0)
            {
                return RedirectToAction("Login", "Acceso");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: PerfilUsuario/ActualizarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarPerfil(Usuario usuario)
        {
            var idUsuario = ObtenerIdUsuario();

            if (idUsuario != usuario.IdUsuario)
            {
                return Forbid();
            }

            // Obtener usuario existente
            var usuarioExistente = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuarioExistente == null)
            {
                return NotFound();
            }

            // Preservar campos que no se deben modificar
            usuario.FechaRegistro = usuarioExistente.FechaRegistro;
            usuario.IdRol = usuarioExistente.IdRol; // No permitir cambiar el rol

            // Si no cambió la clave, mantener la anterior
            if (string.IsNullOrEmpty(usuario.Clave))
            {
                usuario.Clave = usuarioExistente.Clave;
            }

            // Validar que el correo no esté en uso por otro usuario
            var correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo && u.IdUsuario != idUsuario);

            if (correoExiste)
            {
                TempData["Error"] = "El correo electrónico ya está en uso por otro usuario";
                return View("MiPerfil", usuario);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Perfil actualizado exitosamente";

                    // Actualizar el claim del nombre si cambió
                    if (usuarioExistente.Nombres != usuario.Nombres)
                    {
                        return RedirectToAction("Salir", "Acceso");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MiPerfil));
            }

            return View("MiPerfil", usuario);
        }

        // GET: PerfilUsuario/CambiarClave
        public IActionResult CambiarClave()
        {
            return View();
        }

        // POST: PerfilUsuario/CambiarClave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(string claveActual, string claveNueva, string confirmarClave)
        {
            var idUsuario = ObtenerIdUsuario();
            var usuario = await _context.Usuarios.FindAsync(idUsuario);

            if (usuario == null)
            {
                return NotFound();
            }

            // Validar clave actual
            if (usuario.Clave != claveActual)
            {
                TempData["Error"] = "La contraseña actual es incorrecta";
                return View();
            }

            // Validar nueva clave
            if (string.IsNullOrEmpty(claveNueva) || claveNueva.Length < 6)
            {
                TempData["Error"] = "La nueva contraseña debe tener al menos 6 caracteres";
                return View();
            }

            // Validar confirmación
            if (claveNueva != confirmarClave)
            {
                TempData["Error"] = "Las contraseñas no coinciden";
                return View();
            }

            // Actualizar contraseña
            usuario.Clave = claveNueva;
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Contraseña actualizada exitosamente";
            return RedirectToAction(nameof(MiPerfil));
        }
    }
}