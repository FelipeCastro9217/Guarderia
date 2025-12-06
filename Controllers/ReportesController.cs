// Controllers/ReportesController.cs
using Guarderia.Data;
using Guarderia.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Guarderia.Controllers
{
    [Authorize(Roles = "1")] // Solo Administrador
    public class ReportesController : Controller
    {
        private readonly GuarderiaDbContext _context;

        public ReportesController(GuarderiaDbContext context)
        {
            _context = context;
        }

        // GET: Reportes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // Generar PDF de Clientes
        public async Task<IActionResult> GenerarPdfClientes()
        {
            var clientes = await _context.Clientes.ToListAsync();

            using (var flujo = new MemoryStream())
            {
                PdfWriter escribir = new PdfWriter(flujo);
                PdfDocument pdf = new PdfDocument(escribir);
                Document documento = new Document(pdf);

                documento.Add(new Paragraph("Reporte de Clientes")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                Table tabla = new Table(new float[] { 1, 2, 2, 2, 2 }).UseAllAvailableWidth();
                tabla.AddHeaderCell("ID");
                tabla.AddHeaderCell("Nombre");
                tabla.AddHeaderCell("Teléfono");
                tabla.AddHeaderCell("Email");
                tabla.AddHeaderCell("Dirección");

                foreach (var cliente in clientes)
                {
                    tabla.AddCell(cliente.IdCliente.ToString());
                    tabla.AddCell($"{cliente.Nombre} {cliente.Apellido}");
                    tabla.AddCell(cliente.Telefono);
                    tabla.AddCell(cliente.Email);
                    tabla.AddCell(cliente.Direccion);
                }

                documento.Add(tabla);
                documento.Add(new Paragraph("Reporte generado el " +
                    DateTime.Now.ToString("dd/MM/yyyy"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(12));

                documento.Close();
                byte[] pdfBytes = flujo.ToArray();
                return File(pdfBytes, "application/pdf");
            }
        }

        // Mostrar PDF de Clientes en el navegador
        public IActionResult MostrarPdfClientes()
        {
            return View();
        }

        // Generar PDF de Servicios
        public async Task<IActionResult> GenerarPdfServicios()
        {
            var servicios = await _context.InventarioServicios.ToListAsync();

            using (var flujo = new MemoryStream())
            {
                PdfWriter escribir = new PdfWriter(flujo);
                PdfDocument pdf = new PdfDocument(escribir);
                Document documento = new Document(pdf);

                documento.Add(new Paragraph("Reporte de Servicios")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                Table tabla = new Table(new float[] { 1, 2, 2, 1, 1 }).UseAllAvailableWidth();
                tabla.AddHeaderCell("ID");
                tabla.AddHeaderCell("Servicio");
                tabla.AddHeaderCell("Categoría");
                tabla.AddHeaderCell("Precio");
                tabla.AddHeaderCell("Stock");

                foreach (var servicio in servicios)
                {
                    tabla.AddCell(servicio.IdServicio.ToString());
                    tabla.AddCell(servicio.NombreServicio);
                    tabla.AddCell(servicio.Categoria);
                    tabla.AddCell($"${servicio.PrecioUnitario:N2}");
                    tabla.AddCell(servicio.StockDisponible.ToString());
                }

                documento.Add(tabla);
                documento.Add(new Paragraph("Reporte generado el " +
                    DateTime.Now.ToString("dd/MM/yyyy"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(12));

                documento.Close();
                byte[] pdfBytes = flujo.ToArray();
                return File(pdfBytes, "application/pdf");
            }
        }

        // Mostrar PDF de Servicios
        public IActionResult MostrarPdfServicios()
        {
            return View();
        }

        // Generar PDF de Ventas
        public async Task<IActionResult> GenerarPdfVentas()
        {
            var ventas = await _context.VentasServicios
                .Include(v => v.Cliente)
                .Include(v => v.Mascota)
                .ToListAsync();

            using (var flujo = new MemoryStream())
            {
                PdfWriter escribir = new PdfWriter(flujo);
                PdfDocument pdf = new PdfDocument(escribir);
                Document documento = new Document(pdf);

                documento.Add(new Paragraph("Reporte de Ventas")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                Table tabla = new Table(new float[] { 1, 2, 2, 2, 2 }).UseAllAvailableWidth();
                tabla.AddHeaderCell("ID");
                tabla.AddHeaderCell("Fecha");
                tabla.AddHeaderCell("Cliente");
                tabla.AddHeaderCell("Mascota");
                tabla.AddHeaderCell("Total");

                decimal totalGeneral = 0;

                foreach (var venta in ventas)
                {
                    tabla.AddCell(venta.IdVenta.ToString());
                    tabla.AddCell(venta.FechaVenta.ToString("dd/MM/yyyy"));
                    tabla.AddCell($"{venta.Cliente?.Nombre} {venta.Cliente?.Apellido}");
                    tabla.AddCell(venta.Mascota?.Nombre ?? "");
                    tabla.AddCell($"${venta.Total:N2}");
                    totalGeneral += venta.Total;
                }

                // Fila de total
                tabla.AddCell("");
                tabla.AddCell("");
                tabla.AddCell("");
                tabla.AddCell(new Cell().Add(new Paragraph("TOTAL GENERAL:").SetBold()).SetTextAlignment(TextAlignment.RIGHT));
                tabla.AddCell(new Cell().Add(new Paragraph($"${totalGeneral:N2}").SetBold()));

                documento.Add(tabla);

                documento.Add(new Paragraph($"Total de Ventas: {ventas.Count}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12)
                    .SetMarginTop(10));

                documento.Add(new Paragraph($"Monto Total: ${totalGeneral:N2}")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetFontSize(12)
                    .SetBold());

                documento.Add(new Paragraph("Reporte generado el " +
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetFontSize(10));

                documento.Close();
                byte[] pdfBytes = flujo.ToArray();
                return File(pdfBytes, "application/pdf");
            }
        }

        // Mostrar PDF de Ventas
        public IActionResult MostrarPdfVentas()
        {
            return View();
        }
    }
}