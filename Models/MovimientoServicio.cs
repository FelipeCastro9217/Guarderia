// Models/MovimientoServicio.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class MovimientoServicio
    {
        [Key]
        public int IdMovimiento { get; set; }

        [Required]
        [Display(Name = "Servicio")]
        public int IdServicio { get; set; }

        [Required]
        [Display(Name = "Mascota")]
        public int IdMascota { get; set; }

        [Display(Name = "Cuidador Asignado")]
        public int? IdCuidador { get; set; }

        [Required]
        [Display(Name = "Tipo de Movimiento")]
        [StringLength(20)]
        public string TipoMovimiento { get; set; } // "Entrada" o "Salida"

        [Required]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Display(Name = "Fecha de Movimiento")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [StringLength(200)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [ForeignKey("IdServicio")]
        public virtual InventarioServicio? Servicio { get; set; }

        [ForeignKey("IdMascota")]
        public virtual Mascota? Mascota { get; set; }

        [ForeignKey("IdCuidador")]
        public virtual Cuidador? Cuidador { get; set; }
    }
}