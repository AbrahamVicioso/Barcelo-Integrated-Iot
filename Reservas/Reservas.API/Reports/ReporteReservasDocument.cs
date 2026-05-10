using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReporteReservasDocument : IDocument
{
    private readonly List<ReporteReservaItemDto> _items;
    private readonly DateTime? _fechaInicio;
    private readonly DateTime? _fechaFin;

    private static readonly byte[]? _logo = LoadLogo();

    private static byte[]? LoadLogo()
    {
        var asm = typeof(ReporteReservasDocument).Assembly;
        using var stream = asm.GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReporteReservasDocument(List<ReporteReservaItemDto> items, DateTime? fechaInicio, DateTime? fechaFin)
    {
        _items = items;
        _fechaInicio = fechaInicio;
        _fechaFin = fechaFin;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().BorderBottom(2).BorderColor("#1a3c5e").PaddingBottom(8).Row(row =>
            {
                if (_logo != null)
                    row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
                row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
                {
                    c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#1a3c5e");
                    c.Item().Text("Informe de Reservas").FontSize(13).FontColor("#444444");
                });
                row.ConstantItem(200).AlignRight().Column(c =>
                {
                    var periodo = _fechaInicio.HasValue
                        ? $"Período: {_fechaInicio:dd/MM/yyyy} — {_fechaFin:dd/MM/yyyy}"
                        : "Período: Todos los registros";
                    c.Item().Text(periodo).FontSize(9).FontColor("#666666");
                    c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                });
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(12).Column(col =>
        {
            // Resumen
            var totalMonto = _items.Sum(x => x.MontoTotal);
            var totalPagado = _items.Sum(x => x.MontoPagado);

            col.Item().PaddingBottom(12).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Total Reservas").FontSize(8).FontColor("#666666");
                    c.Item().Text(_items.Count.ToString()).FontSize(16).Bold().FontColor("#1a3c5e");
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Ingresos Totales").FontSize(8).FontColor("#666666");
                    c.Item().Text($"${totalMonto:N2}").FontSize(16).Bold().FontColor("#1a3c5e");
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Ingresos Cobrados").FontSize(8).FontColor("#666666");
                    c.Item().Text($"${totalPagado:N2}").FontSize(16).Bold().FontColor("#2e7d32");
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Pendiente por Cobrar").FontSize(8).FontColor("#666666");
                    c.Item().Text($"${(totalMonto - totalPagado):N2}").FontSize(16).Bold().FontColor("#c62828");
                });
            });

            // Tabla
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(90);  // N° Reserva
                    cols.RelativeColumn(2);   // Huésped
                    cols.ConstantColumn(75);  // Check-In
                    cols.ConstantColumn(75);  // Check-Out
                    cols.ConstantColumn(80);  // Monto Total
                    cols.ConstantColumn(80);  // Monto Pagado
                    cols.ConstantColumn(70);  // Estado
                });

                // Header
                table.Header(header =>
                {
                    static void HeaderCell(IContainer c, string text) =>
                        c.Background("#1a3c5e").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                    header.Cell().Element(c => HeaderCell(c, "N° Reserva"));
                    header.Cell().Element(c => HeaderCell(c, "Huésped"));
                    header.Cell().Element(c => HeaderCell(c, "Check-In"));
                    header.Cell().Element(c => HeaderCell(c, "Check-Out"));
                    header.Cell().Element(c => HeaderCell(c, "Monto Total"));
                    header.Cell().Element(c => HeaderCell(c, "Monto Pagado"));
                    header.Cell().Element(c => HeaderCell(c, "Estado"));
                });

                // Rows
                foreach (var (item, index) in _items.Select((x, i) => (x, i)))
                {
                    var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";

                    static IContainer Cell(IContainer c, string bg) =>
                        c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                    table.Cell().Element(c => Cell(c, bg)).Text(item.NumeroReserva).FontSize(8);
                    table.Cell().Element(c => Cell(c, bg)).Text(item.Huesped).FontSize(8);
                    table.Cell().Element(c => Cell(c, bg)).Text(item.FechaCheckIn.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                    table.Cell().Element(c => Cell(c, bg)).Text(item.FechaCheckOut.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                    table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"${item.MontoTotal:N2}").FontSize(8);
                    table.Cell().Element(c => Cell(c, bg)).AlignRight().Text($"${item.MontoPagado:N2}").FontSize(8);

                    var estadoColor = item.Estado switch
                    {
                        "Activa"   => "#2e7d32",
                        "Pendiente"=> "#e65100",
                        "CheckOut" => "#1565c0",
                        _          => "#555555"
                    };
                    table.Cell().Element(c => Cell(c, bg)).Text(item.Estado).FontSize(8).FontColor(estadoColor);
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor("#e0e0e0").PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text("SmartStay — Documento confidencial").FontSize(7).FontColor("#999999");
            row.ConstantItem(100).AlignRight().Text(x =>
            {
                x.Span("Página ").FontSize(7).FontColor("#999999");
                x.CurrentPageNumber().FontSize(7).FontColor("#999999");
                x.Span(" de ").FontSize(7).FontColor("#999999");
                x.TotalPages().FontSize(7).FontColor("#999999");
            });
        });
    }
}
