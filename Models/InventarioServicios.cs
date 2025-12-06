// Models/InventarioServicio.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class InventarioServicio
    {
        [Key]
        public int IdServicio { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Nombre del Servicio")]
        public string NombreServicio { get; set; }

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Display(Name = "Stock Disponible")]
        public int StockDisponible { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } // Baño, Hospedaje, Veterinario, etc.

        public virtual ICollection<DetalleVentaServicio>? DetallesVentas { get; set; }
        public virtual ICollection<MovimientoServicio>? MovimientosServicios { get; set; }
    }
}
