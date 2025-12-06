// Models/DetalleVentaServicio.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class DetalleVentaServicio
    {
        [Key]
        public int IdDetalle { get; set; }

        [Required]
        [Display(Name = "Venta")]
        public int IdVenta { get; set; }

        [Required]
        [Display(Name = "Servicio")]
        public int IdServicio { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [ForeignKey("IdVenta")]
        public virtual VentaServicio? Venta { get; set; }

        [ForeignKey("IdServicio")]
        public virtual InventarioServicio? Servicio { get; set; }
    }
}
