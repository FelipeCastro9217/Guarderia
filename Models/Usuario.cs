using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Nombres")]
        public string Nombres { get; set; }

        [Required, StringLength(100), EmailAddress]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Contraseña")]
        public string Clave { get; set; }

        [Required]
        [Display(Name = "Rol")]
        public int IdRol { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }
    }
}
