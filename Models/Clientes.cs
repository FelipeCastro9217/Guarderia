// Models/Clientes.cs
using System.ComponentModel.DataAnnotations;

namespace Guarderia.Models
{
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        [StringLength(50)]
        public string Apellido { get; set; }

        [StringLength(15)]
        public string Telefono { get; set; }

        [StringLength(100), EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string Direccion { get; set; }

        // Nuevos campos para autenticación
        [StringLength(100)]
        [Display(Name = "Contraseña")]
        public string? Clave { get; set; }

        [Display(Name = "Cuenta Activa")]
        public bool CuentaActiva { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Relaciones
        public virtual ICollection<Mascota>? Mascotas { get; set; }
    }
}