// Controllers/AccesoController.cs
using Guarderia.Logica;
using Guarderia.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Guarderia.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccesoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: Acceso/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Acceso/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Usuario _usuario)
        {
            var connectionString = _configuration.GetConnectionString("ConexionBD");
            var logicaUsuarios = new Logica_Usuarios(connectionString);
            var usuario = logicaUsuarios.EncontrarUsuario(_usuario.Correo, _usuario.Clave);

            if (usuario != null && !string.IsNullOrEmpty(usuario.Nombres))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombres),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.IdRol.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Mensaje"] = $"Bienvenido, {usuario.Nombres}";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Error"] = "Correo o contraseña incorrectos";
                return View();
            }
        }

        // GET: Acceso/Salir
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login", "Acceso");
        }

        // GET: Acceso/AccesoDenegado
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
