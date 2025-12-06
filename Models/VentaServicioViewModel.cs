// Models/ViewModels/VentaServicioViewModel.cs
namespace Guarderia.Models.ViewModels
{
    public class VentaServicioViewModel
    {
        public int IdCliente { get; set; }
        public int IdMascota { get; set; }
        public List<DetalleServicioViewModel> Servicios { get; set; } = new List<DetalleServicioViewModel>();
    }

    public class DetalleServicioViewModel
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}