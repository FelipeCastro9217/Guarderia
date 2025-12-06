using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class DetalleFactura
    {
        [Key]
        public int IdDetalleFactura { get; set; }

        [Required]
        [Display(Name = "Factura")]
        public int IdFactura { get; set; }

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
        [Display(Name = "Descuento por Item")]
        public decimal DescuentoItem { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [ForeignKey("IdFactura")]
        public virtual Factura? Factura { get; set; }

        [ForeignKey("IdServicio")]
        public virtual InventarioServicio? Servicio { get; set; }
    }
}
