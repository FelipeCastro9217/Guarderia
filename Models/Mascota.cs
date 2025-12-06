// Models/Mascota.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class Mascota
    {
        [Key]
        public int IdMascota { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Raza")]
        public string Raza { get; set; }

        [Required]
        [Display(Name = "Edad (años)")]
        public int Edad { get; set; }

        [Required]
        [Display(Name = "Peso (kg)")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Peso { get; set; }

        [StringLength(500)]
        [Display(Name = "Historial Médico")]
        public string? HistorialMedico { get; set; }

        [Required]
        [Display(Name = "Cliente")]
        public int IdCliente { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        public virtual ICollection<MovimientoServicio>? MovimientosServicios { get; set; }
    }
}
