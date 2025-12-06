namespace Guarderia.Models.ViewModels
{
    public class FacturaViewModel
    {
        public int IdCliente { get; set; }
        public int IdMascota { get; set; }
        public decimal DescuentoGlobal { get; set; }
        public string? Observaciones { get; set; }
        public List<DetalleFacturaTemp> Servicios { get; set; } = new List<DetalleFacturaTemp>();
    }

    public class DetalleFacturaTemp
    {
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal DescuentoItem { get; set; }
    }
}
