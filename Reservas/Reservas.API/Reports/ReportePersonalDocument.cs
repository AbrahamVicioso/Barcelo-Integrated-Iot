using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReportePersonalDocument : IDocument
{
    private readonly List<ReportePersonalItemDto> _items;
    private readonly bool? _soloActivos;

    private static readonly byte[]? _logo = LoadLogo();
    private static byte[]? LoadLogo()
    {
        using var stream = typeof(ReportePersonalDocument).Assembly
            .GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReportePersonalDocument(List<ReportePersonalItemDto> items, bool? soloActivos)
    {
        _items = items;
        _soloActivos = soloActivos;
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
        container.BorderBottom(2).BorderColor("#37474f").PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#37474f");
                c.Item().Text("Reporte de Personal").FontSize(13).FontColor("#444444");
                var subtitulo = _soloActivos switch
                {
                    true  => "Solo personal activo",
                    false => "Solo personal inactivo",
                    null  => "Todo el personal"
                };
                c.Item().PaddingTop(2).Text(subtitulo).FontSize(9).FontColor("#888888").Italic();
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total: {_items.Count} | Activos: {_items.Count(x => x.EstaActivo)}").FontSize(9).Bold().FontColor("#37474f");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontró personal con los filtros aplicados.")
                .FontSize(11).FontColor("#999999").Italic();
            return;
        }

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2.5f);// Nombre
                cols.ConstantColumn(80);  // N° Empleado
                cols.RelativeColumn(1.5f);// Puesto
                cols.RelativeColumn(1.5f);// Departamento
                cols.RelativeColumn(1.5f);// Hotel
                cols.ConstantColumn(60);  // Turno
                cols.ConstantColumn(85);  // Contratación
                cols.ConstantColumn(55);  // Activo
            });

            table.Header(header =>
            {
                void HeaderCell(IContainer c, string text) =>
                    c.Background("#37474f").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                header.Cell().Element(c => HeaderCell(c, "Nombre Completo"));
                header.Cell().Element(c => HeaderCell(c, "N° Empleado"));
                header.Cell().Element(c => HeaderCell(c, "Puesto"));
                header.Cell().Element(c => HeaderCell(c, "Departamento"));
                header.Cell().Element(c => HeaderCell(c, "Hotel"));
                header.Cell().Element(c => HeaderCell(c, "Turno"));
                header.Cell().Element(c => HeaderCell(c, "Contratación"));
                header.Cell().Element(c => HeaderCell(c, "Activo"));
            });

            foreach (var (item, index) in _items.Select((x, i) => (x, i)))
            {
                var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";
                IContainer Cell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                table.Cell().Element(Cell).Text(item.NombreCompleto).FontSize(8);
                table.Cell().Element(Cell).AlignCenter().Text(item.NumeroEmpleado).FontSize(8);
                table.Cell().Element(Cell).Text(item.Puesto).FontSize(8);
                table.Cell().Element(Cell).Text(item.Departamento).FontSize(8);
                table.Cell().Element(Cell).Text(item.Hotel).FontSize(8);
                table.Cell().Element(Cell).AlignCenter().Text(string.IsNullOrEmpty(item.Turno) ? "-" : item.Turno).FontSize(8);
                table.Cell().Element(Cell).Text(item.FechaContratacion.ToString("dd/MM/yyyy")).FontSize(8);
                table.Cell().Element(Cell).AlignCenter()
                    .Text(item.EstaActivo ? "Activo" : "Inactivo").FontSize(8)
                    .FontColor(item.EstaActivo ? "#2e7d32" : "#c62828");
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
