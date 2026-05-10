using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReporteActividadesSinAccesoDocument : IDocument
{
    private readonly List<ReporteActividadSinAccesoItemDto> _items;
    private readonly DateTime? _fechaInicio;
    private readonly DateTime? _fechaFin;

    private static readonly byte[]? _logo = LoadLogo();
    private static byte[]? LoadLogo()
    {
        using var stream = typeof(ReporteActividadesSinAccesoDocument).Assembly
            .GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReporteActividadesSinAccesoDocument(List<ReporteActividadSinAccesoItemDto> items, DateTime? fechaInicio, DateTime? fechaFin)
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
        container.BorderBottom(2).BorderColor("#b71c1c").PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#b71c1c");
                c.Item().Text("Actividades Sin Acceso — Huéspedes No Asistidos").FontSize(13).FontColor("#444444");
                c.Item().PaddingTop(2).Text("Reservas de actividades con fecha pasada que permanecen pendientes").FontSize(9).FontColor("#888888").Italic();
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                var periodo = _fechaInicio.HasValue
                    ? $"Período: {_fechaInicio:dd/MM/yyyy} — {_fechaFin:dd/MM/yyyy}"
                    : "Período: Todos los registros";
                c.Item().Text(periodo).FontSize(9).FontColor("#666666");
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total: {_items.Count}").FontSize(9).Bold().FontColor("#b71c1c");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontraron reservas de actividades sin acceso.")
                .FontSize(11).FontColor("#999999").Italic();
            return;
        }

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2);   // Actividad
                cols.RelativeColumn(2);   // Huésped
                cols.ConstantColumn(90);  // Fecha Reserva
                cols.ConstantColumn(60);  // Hora
                cols.ConstantColumn(65);  // Personas
                cols.RelativeColumn(1.2f);// Estado
                cols.ConstantColumn(80);  // Monto
            });

            table.Header(header =>
            {
                void HeaderCell(IContainer c, string text) =>
                    c.Background("#b71c1c").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                header.Cell().Element(c => HeaderCell(c, "Actividad"));
                header.Cell().Element(c => HeaderCell(c, "Huésped"));
                header.Cell().Element(c => HeaderCell(c, "Fecha Reserva"));
                header.Cell().Element(c => HeaderCell(c, "Hora"));
                header.Cell().Element(c => HeaderCell(c, "Personas"));
                header.Cell().Element(c => HeaderCell(c, "Estado"));
                header.Cell().Element(c => HeaderCell(c, "Monto"));
            });

            foreach (var (item, index) in _items.Select((x, i) => (x, i)))
            {
                var bg = index % 2 == 0 ? "#ffffff" : "#fff5f5";
                IContainer Cell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                table.Cell().Element(Cell).Text(item.NombreActividad).FontSize(8);
                table.Cell().Element(Cell).Text(item.Huesped).FontSize(8);
                table.Cell().Element(Cell).Text(item.FechaReserva.ToString("dd/MM/yyyy")).FontSize(8);
                table.Cell().Element(Cell).AlignCenter().Text(item.HoraReserva.ToString(@"hh\:mm")).FontSize(8);
                table.Cell().Element(Cell).AlignCenter().Text(item.NumeroPersonas.ToString()).FontSize(8);
                table.Cell().Element(Cell).Text(item.Estado).FontSize(8).FontColor("#c62828");
                table.Cell().Element(Cell).AlignRight().Text($"${item.MontoTotal:N2}").FontSize(8);
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
