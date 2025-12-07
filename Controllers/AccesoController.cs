// Controllers/AccesoController.cs
using Guarderia.Data;
using Guarderia.Logica;
using Guarderia.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Guarderia.Controllers
{
    public class AccesoController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly GuarderiaDbContext _context;

        public AccesoController(IConfiguration configuration, GuarderiaDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // GET: Acceso/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Acceso/Login (Administradores y Empleados)
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
                    new Claim(ClaimTypes.Role, usuario.IdRol.ToString()),
                    new Claim("TipoUsuario", "Empleado")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
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

        // POST: Acceso/LoginCliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginCliente(string Email, string Clave)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == Email && c.Clave == Clave);

            if (cliente != null && cliente.CuentaActiva)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, cliente.IdCliente.ToString()),
                    new Claim(ClaimTypes.Name, $"{cliente.Nombre} {cliente.Apellido}"),
                    new Claim(ClaimTypes.Email, cliente.Email),
                    new Claim(ClaimTypes.Role, "Cliente"),
                    new Claim("TipoUsuario", "Cliente")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Mensaje"] = $"Bienvenido, {cliente.Nombre}";
                return RedirectToAction("Dashboard", "PanelCliente");
            }
            else
            {
                TempData["Error"] = cliente == null ?
                    "Correo o contraseña incorrectos" :
                    "Tu cuenta no está activa. Contacta con la guardería.";
                return RedirectToAction("Login");
            }
        }

        // GET: Acceso/Registro
        public IActionResult Registro()
        {
            return View();
        }

        // POST: Acceso/RegistroCompleto (Cliente + Mascota)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistroCompleto()
        {
            try
            {
                // Validar contraseña
                var clave = Request.Form["Cliente.Clave"].ToString();
                if (string.IsNullOrEmpty(clave) || clave.Length < 6)
                {
                    TempData["Error"] = "La contraseña debe tener al menos 6 caracteres";
                    return View("Registro");
                }

                // Crear objeto Cliente
                var cliente = new Cliente
                {
                    Nombre = Request.Form["Cliente.Nombre"],
                    Apellido = Request.Form["Cliente.Apellido"],
                    Email = Request.Form["Cliente.Email"],
                    Telefono = Request.Form["Cliente.Telefono"],
                    Direccion = Request.Form["Cliente.Direccion"],
                    Clave = clave,
                    CuentaActiva = true,
                    FechaRegistro = DateTime.Now
                };

                // Validar que no exista el correo
                var correoExiste = await _context.Clientes
                    .AnyAsync(c => c.Email == cliente.Email);

                if (correoExiste)
                {
                    TempData["Error"] = "El correo electrónico ya está registrado";
                    return View("Registro");
                }

                // Validar que no exista el teléfono
                var telefonoExiste = await _context.Clientes
                    .AnyAsync(c => c.Telefono == cliente.Telefono);

                if (telefonoExiste)
                {
                    TempData["Error"] = "El número de teléfono ya está registrado";
                    return View("Registro");
                }

                // Guardar el cliente
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                // Crear la mascota
                var mascota = new Mascota
                {
                    Nombre = Request.Form["Mascota.Nombre"],
                    Raza = Request.Form["Mascota.Raza"],
                    Edad = int.Parse(Request.Form["Mascota.Edad"]),
                    Peso = decimal.Parse(Request.Form["Mascota.Peso"]),
                    HistorialMedico = Request.Form["Mascota.HistorialMedico"],
                    IdCliente = cliente.IdCliente
                };

                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"¡Registro exitoso! Ya puedes iniciar sesión con tu correo: {cliente.Email}";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar: {ex.Message}";
                return View("Registro");
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