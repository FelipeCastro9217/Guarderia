// Models/Cuidador.cs
using System.ComponentModel.DataAnnotations;

namespace Guarderia.Models
{
    public class Cuidador
    {
        [Key]
        public int IdCuidador { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required, StringLength(15)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required, StringLength(100), EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100)]
        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [Display(Name = "Fecha de Contratación")]
        public DateTime FechaContratacion { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}