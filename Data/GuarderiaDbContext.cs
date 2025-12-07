// Data/GuarderiaDbContext.cs
using Guarderia.Models;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Data
{
    public class GuarderiaDbContext : DbContext
    {
        public GuarderiaDbContext(DbContextOptions<GuarderiaDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<InventarioServicio> InventarioServicios { get; set; }
        public DbSet<MovimientoServicio> MovimientosServicios { get; set; }
        public DbSet<VentaServicio> VentasServicios { get; set; }
        public DbSet<DetalleVentaServicio> DetallesVentasServicios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFacturas { get; set; }
        public DbSet<Cuidador> Cuidadores { get; set; }
        public DbSet<Agendamiento> Agendamientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones existentes
            modelBuilder.Entity<Mascota>()
                .HasOne(m => m.Cliente)
                .WithMany(c => c.Mascotas)
                .HasForeignKey(m => m.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VentaServicio>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VentaServicio>()
                .HasOne(v => v.Mascota)
                .WithMany()
                .HasForeignKey(v => v.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleVentaServicio>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.DetallesVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleVentaServicio>()
                .HasOne(d => d.Servicio)
                .WithMany(s => s.DetallesVentas)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoServicio>()
                .HasOne(m => m.Servicio)
                .WithMany(s => s.MovimientosServicios)
                .HasForeignKey(m => m.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoServicio>()
                .HasOne(m => m.Mascota)
                .WithMany(ma => ma.MovimientosServicios)
                .HasForeignKey(m => m.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MovimientoServicio>()
                .HasOne(m => m.Cuidador)
                .WithMany()
                .HasForeignKey(m => m.IdCuidador)
                .OnDelete(DeleteBehavior.SetNull);

            // Configuración de Agendamientos
            modelBuilder.Entity<Agendamiento>()
                .HasOne(a => a.Cliente)
                .WithMany()
                .HasForeignKey(a => a.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamiento>()
                .HasOne(a => a.Mascota)
                .WithMany()
                .HasForeignKey(a => a.IdMascota)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Agendamiento>()
                .HasOne(a => a.Servicio)
                .WithMany()
                .HasForeignKey(a => a.IdServicio)
                .OnDelete(DeleteBehavior.Restrict);

            // Datos iniciales de Servicios
            modelBuilder.Entity<InventarioServicio>().HasData(
                new InventarioServicio
                {
                    IdServicio = 1,
                    NombreServicio = "Baño Completo",
                    Descripcion = "Baño con shampoo especial, secado y cepillado",
                    PrecioUnitario = 35000,
                    StockDisponible = 50,
                    Categoria = "Baño"
                },
                new InventarioServicio
                {
                    IdServicio = 2,
                    NombreServicio = "Hospedaje Diario",
                    Descripcion = "Alojamiento por día con alimentación incluida",
                    PrecioUnitario = 45000,
                    StockDisponible = 20,
                    Categoria = "Hospedaje"
                },
                new InventarioServicio
                {
                    IdServicio = 3,
                    NombreServicio = "Paseo en Parque",
                    Descripcion = "Paseo de 1 hora en parque",
                    PrecioUnitario = 15000,
                    StockDisponible = 100,
                    Categoria = "Paseo"
                },
                new InventarioServicio
                {
                    IdServicio = 4,
                    NombreServicio = "Consulta Veterinaria",
                    Descripcion = "Revisión general y diagnóstico",
                    PrecioUnitario = 50000,
                    StockDisponible = 30,
                    Categoria = "Veterinario"
                },
                new InventarioServicio
                {
                    IdServicio = 5,
                    NombreServicio = "Corte de Uñas",
                    Descripcion = "Corte y limado de uñas",
                    PrecioUnitario = 10000,
                    StockDisponible = 80,
                    Categoria = "Estética"
                }
            );

            // Datos iniciales de Roles
            modelBuilder.Entity<Rol>().HasData(
                new Rol { IdRol = 1, NombreRol = "Administrador", Descripcion = "Acceso total al sistema" },
                new Rol { IdRol = 2, NombreRol = "Empleado", Descripcion = "Acceso limitado" },
                new Rol { IdRol = 3, NombreRol = "Cliente", Descripcion = "Acceso al panel de cliente" }
            );

            // Datos iniciales de Usuarios
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    IdUsuario = 1,
                    Nombres = "Admin Sistema",
                    Correo = "admin@guarderia.com",
                    Clave = "admin",
                    IdRol = 1,
                    FechaRegistro = DateTime.Now
                },
                new Usuario
                {
                    IdUsuario = 2,
                    Nombres = "Empleado Uno",
                    Correo = "empleado@guarderia.com",
                    Clave = "123",
                    IdRol = 2,
                    FechaRegistro = DateTime.Now
                }
            );

            // Datos iniciales de Cuidadores
            modelBuilder.Entity<Cuidador>().HasData(
                new Cuidador
                {
                    IdCuidador = 1,
                    Nombre = "Carlos",
                    Apellido = "Rodríguez",
                    Telefono = "3001234567",
                    Email = "carlos.rodriguez@guarderia.com",
                    Especialidad = "Perros grandes",
                    FechaContratacion = DateTime.Now.AddMonths(-12),
                    Activo = true
                },
                new Cuidador
                {
                    IdCuidador = 2,
                    Nombre = "María",
                    Apellido = "López",
                    Telefono = "3009876543",
                    Email = "maria.lopez@guarderia.com",
                    Especialidad = "Perros pequeños",
                    FechaContratacion = DateTime.Now.AddMonths(-6),
                    Activo = true
                }
            );
        }
    }
}