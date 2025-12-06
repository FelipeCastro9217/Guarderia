using System.ComponentModel.DataAnnotations;

namespace Guarderia.Models
{
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; }

        [StringLength(200)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        public virtual ICollection<Usuario>? Usuarios { get; set; }
    }
}
