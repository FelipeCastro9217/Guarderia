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
                // Crear objeto Cliente desde el formulario
                var cliente = new Cliente
                {
                    Nombre = Request.Form["Cliente.Nombre"],
                    Apellido = Request.Form["Cliente.Apellido"],
                    Email = Request.Form["Cliente.Email"],
                    Telefono = Request.Form["Cliente.Telefono"],
                    Direccion = Request.Form["Cliente.Direccion"],
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

                // Guardar el cliente primero
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                // Ahora crear la mascota asociada al cliente recién creado
                var mascota = new Mascota
                {
                    Nombre = Request.Form["Mascota.Nombre"],
                    Raza = Request.Form["Mascota.Raza"],
                    Edad = int.Parse(Request.Form["Mascota.Edad"]),
                    Peso = decimal.Parse(Request.Form["Mascota.Peso"]),
                    HistorialMedico = Request.Form["Mascota.HistorialMedico"],
                    IdCliente = cliente.IdCliente // Usar el ID del cliente recién creado
                };

                _context.Mascotas.Add(mascota);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = $"¡Registro exitoso! Cliente '{cliente.Nombre}' y mascota '{mascota.Nombre}' registrados correctamente. Por favor contacte con la guardería para agendar servicios.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar: {ex.Message}";
                return View("Registro");
            }
        }

        // POST: Acceso/Registro (Registro simple desde pestaña del login)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el correo ya existe
                    var clienteExistente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Email == cliente.Email);

                    if (clienteExistente != null)
                    {
                        TempData["Error"] = "El correo electrónico ya está registrado";
                        return View(cliente);
                    }

                    // Verificar si el teléfono ya existe
                    var telefonoExistente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Telefono == cliente.Telefono);

                    if (telefonoExistente != null)
                    {
                        TempData["Error"] = "El número de teléfono ya está registrado";
                        return View(cliente);
                    }

                    // Establecer la fecha de registro
                    cliente.FechaRegistro = DateTime.Now;

                    // Guardar el cliente
                    _context.Clientes.Add(cliente);
                    await _context.SaveChangesAsync();

                    TempData["Mensaje"] = "Registro exitoso. Por favor contacte con la guardería para registrar su mascota y activar servicios.";
                    return RedirectToAction(nameof(Login));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al registrar: {ex.Message}";
                    return View(cliente);
                }
            }

            return View(cliente);
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