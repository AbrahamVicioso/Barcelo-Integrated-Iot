using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReporteHabitacionesDocument : IDocument
{
    private readonly List<ReporteHabitacionItemDto> _items;
    private readonly string _filtros;

    private static readonly byte[]? _logo = LoadLogo();
    private static byte[]? LoadLogo()
    {
        using var stream = typeof(ReporteHabitacionesDocument).Assembly
            .GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReporteHabitacionesDocument(List<ReporteHabitacionItemDto> items, string filtros)
    {
        _items = items;
        _filtros = filtros;
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
        container.BorderBottom(2).BorderColor("#1a3c5e").PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#1a3c5e");
                c.Item().Text("Reporte de Habitaciones").FontSize(13).FontColor("#444444");
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                c.Item().Text(_filtros).FontSize(9).FontColor("#666666");
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total habitaciones: {_items.Count}").FontSize(9).Bold().FontColor("#1a3c5e");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontraron habitaciones con los filtros aplicados.")
                .FontSize(11).FontColor("#999999").Italic();
            return;
        }

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);   // Hotel
                cols.ConstantColumn(70);  // N° Hab
                cols.RelativeColumn(1.5f);// Tipo
                cols.RelativeColumn(1.5f);// Estado
                cols.ConstantColumn(40);  // Piso
                cols.ConstantColumn(70);  // Capacidad
                cols.ConstantColumn(85);  // Precio/noche
            });

            table.Header(header =>
            {
                void HeaderCell(IContainer c, string text) =>
                    c.Background("#1a3c5e").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                header.Cell().Element(c => HeaderCell(c, "Hotel"));
                header.Cell().Element(c => HeaderCell(c, "N° Hab."));
                header.Cell().Element(c => HeaderCell(c, "Tipo"));
                header.Cell().Element(c => HeaderCell(c, "Estado"));
                header.Cell().Element(c => HeaderCell(c, "Piso"));
                header.Cell().Element(c => HeaderCell(c, "Capacidad"));
                header.Cell().Element(c => HeaderCell(c, "Precio/Noche"));
            });

            foreach (var (item, index) in _items.Select((x, i) => (x, i)))
            {
                var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";
                IContainer Cell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                var estadoColor = item.Estado.ToLower() switch
                {
                    var e when e.Contains("disponible") => "#2e7d32",
                    var e when e.Contains("ocupada")    => "#c62828",
                    var e when e.Contains("mantenimiento") => "#e65100",
                    _ => "#555555"
                };

                table.Cell().Element(Cell).Text(item.Hotel).FontSize(8);
                table.Cell().Element(Cell).Text(item.NumeroHabitacion).FontSize(8);
                table.Cell().Element(Cell).Text(item.Tipo).FontSize(8);
                table.Cell().Element(Cell).Text(item.Estado).FontSize(8).FontColor(estadoColor);
                table.Cell().Element(Cell).AlignCenter().Text(item.Piso.ToString()).FontSize(8);
                table.Cell().Element(Cell).AlignCenter().Text($"{item.CapacidadMaxima} pers.").FontSize(8);
                table.Cell().Element(Cell).AlignRight().Text($"${item.PrecioPorNoche:N2}").FontSize(8);
            }
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
