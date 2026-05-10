using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReporteActividadesResumenDocument : IDocument
{
    private readonly List<ReporteActividadResumenItemDto> _items;
    private readonly DateTime? _fechaInicio;
    private readonly DateTime? _fechaFin;

    private static readonly byte[]? _logo = LoadLogo();
    private static byte[]? LoadLogo()
    {
        using var stream = typeof(ReporteActividadesResumenDocument).Assembly
            .GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReporteActividadesResumenDocument(List<ReporteActividadResumenItemDto> items, DateTime? fechaInicio, DateTime? fechaFin)
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
        container.BorderBottom(2).BorderColor("#4a148c").PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#4a148c");
                c.Item().Text("Resumen de Actividades Recreativas").FontSize(13).FontColor("#444444");
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                var periodo = _fechaInicio.HasValue
                    ? $"Período: {_fechaInicio:dd/MM/yyyy} — {_fechaFin:dd/MM/yyyy}"
                    : "Período: Todos los registros";
                c.Item().Text(periodo).FontSize(9).FontColor("#666666");
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total actividades: {_items.Count}").FontSize(9).Bold().FontColor("#4a148c");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontraron actividades.").FontSize(11).FontColor("#999999").Italic();
            return;
        }

        // KPI summary
        container.PaddingTop(12).Column(col =>
        {
            col.Item().PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Total Reservas").FontSize(8).FontColor("#666666");
                    c.Item().Text(_items.Sum(x => x.TotalReservas).ToString()).FontSize(16).Bold().FontColor("#4a148c");
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Total Personas").FontSize(8).FontColor("#666666");
                    c.Item().Text(_items.Sum(x => x.TotalPersonas).ToString()).FontSize(16).Bold().FontColor("#1565c0");
                });
                row.ConstantItem(10);
                row.RelativeItem().Border(1).BorderColor("#e0e0e0").Padding(8).Column(c =>
                {
                    c.Item().Text("Ingresos Totales").FontSize(8).FontColor("#666666");
                    c.Item().Text($"${_items.Sum(x => x.TotalIngresos):N2}").FontSize(16).Bold().FontColor("#2e7d32");
                });
            });

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);   // Actividad
                    cols.RelativeColumn(1.5f);// Categoría
                    cols.RelativeColumn(2);   // Hotel
                    cols.ConstantColumn(75);  // Reservas
                    cols.ConstantColumn(75);  // Personas
                    cols.ConstantColumn(90);  // Ingresos
                    cols.ConstantColumn(55);  // Activa
                });

                table.Header(header =>
                {
                    void HeaderCell(IContainer c, string text) =>
                        c.Background("#4a148c").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                    header.Cell().Element(c => HeaderCell(c, "Actividad"));
                    header.Cell().Element(c => HeaderCell(c, "Categoría"));
                    header.Cell().Element(c => HeaderCell(c, "Hotel"));
                    header.Cell().Element(c => HeaderCell(c, "Reservas"));
                    header.Cell().Element(c => HeaderCell(c, "Personas"));
                    header.Cell().Element(c => HeaderCell(c, "Ingresos"));
                    header.Cell().Element(c => HeaderCell(c, "Activa"));
                });

                foreach (var (item, index) in _items.Select((x, i) => (x, i)))
                {
                    var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";
                    IContainer Cell(IContainer c) =>
                        c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                    table.Cell().Element(Cell).Text(item.NombreActividad).FontSize(8);
                    table.Cell().Element(Cell).Text(item.Categoria).FontSize(8);
                    table.Cell().Element(Cell).Text(item.Hotel).FontSize(8);
                    table.Cell().Element(Cell).AlignCenter().Text(item.TotalReservas.ToString()).FontSize(8);
                    table.Cell().Element(Cell).AlignCenter().Text(item.TotalPersonas.ToString()).FontSize(8);
                    table.Cell().Element(Cell).AlignRight().Text($"${item.TotalIngresos:N2}").FontSize(8);
                    table.Cell().Element(Cell).AlignCenter()
                        .Text(item.EstaActiva ? "Sí" : "No").FontSize(8)
                        .FontColor(item.EstaActiva ? "#2e7d32" : "#c62828");
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
