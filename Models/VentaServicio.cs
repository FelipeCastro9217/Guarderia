// Models/VentaServicio.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class VentaServicio
    {
        [Key]
        public int IdVenta { get; set; }

        [Required]
        [Display(Name = "Cliente")]
        public int IdCliente { get; set; }

        [Required]
        [Display(Name = "Mascota")]
        public int IdMascota { get; set; }

        [Required]
        [Display(Name = "Fecha de Venta")]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        [StringLength(50)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Completada"; // Completada, Pendiente, Cancelada

        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("IdMascota")]
        public virtual Mascota? Mascota { get; set; }

        public virtual ICollection<DetalleVentaServicio>? DetallesVenta { get; set; }
    }
}
