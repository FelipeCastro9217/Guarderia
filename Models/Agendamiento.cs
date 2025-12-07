// Models/Agendamiento.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Guarderia.Models
{
    public class Agendamiento
    {
        [Key]
        public int IdAgendamiento { get; set; }

        [Required]
        [Display(Name = "Cliente")]
        public int IdCliente { get; set; }

        [Required]
        [Display(Name = "Mascota")]
        public int IdMascota { get; set; }

        [Required]
        [Display(Name = "Servicio")]
        public int IdServicio { get; set; }

        [Required]
        [Display(Name = "Fecha Solicitada")]
        public DateTime FechaSolicitada { get; set; }

        [Required]
        [Display(Name = "Hora Solicitada")]
        [StringLength(10)]
        public string HoraSolicitada { get; set; }

        [Required]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; } = 1;

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmado, Completado, Cancelado

        [Display(Name = "Fecha de Solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Confirmado Por")]
        public int? IdUsuarioConfirmacion { get; set; }

        [Display(Name = "Fecha de Confirmación")]
        public DateTime? FechaConfirmacion { get; set; }

        [StringLength(200)]
        [Display(Name = "Notas del Administrador")]
        public string? NotasAdmin { get; set; }

        // Navegación
        [ForeignKey("IdCliente")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("IdMascota")]
        public virtual Mascota? Mascota { get; set; }

        [ForeignKey("IdServicio")]
        public virtual InventarioServicio? Servicio { get; set; }

        [ForeignKey("IdUsuarioConfirmacion")]
        public virtual Usuario? UsuarioConfirmacion { get; set; }
    }
}