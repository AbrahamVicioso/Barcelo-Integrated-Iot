using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReportePuntualidadDocument : IDocument
{
    private readonly List<ReportePuntualidadItemDto> _items;
    private readonly string _titulo;
    private readonly string _subtitulo;
    private readonly DateTime? _fechaInicio;
    private readonly DateTime? _fechaFin;
    private readonly string _accentColor;

    private static readonly byte[]? _logo = LoadLogo();

    private static byte[]? LoadLogo()
    {
        var asm = typeof(ReportePuntualidadDocument).Assembly;
        using var stream = asm.GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReportePuntualidadDocument(
        List<ReportePuntualidadItemDto> items,
        string titulo,
        string subtitulo,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        string accentColor = "#1a3c5e")
    {
        _items = items;
        _titulo = titulo;
        _subtitulo = subtitulo;
        _fechaInicio = fechaInicio;
        _fechaFin = fechaFin;
        _accentColor = accentColor;
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
        container.BorderBottom(2).BorderColor(_accentColor).PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor(_accentColor);
                c.Item().Text(_titulo).FontSize(13).FontColor("#444444");
                c.Item().PaddingTop(2).Text(_subtitulo).FontSize(9).FontColor("#888888").Italic();
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                var periodo = _fechaInicio.HasValue
                    ? $"Período: {_fechaInicio:dd/MM/yyyy} — {_fechaFin:dd/MM/yyyy}"
                    : "Período: Todos los registros";
                c.Item().Text(periodo).FontSize(9).FontColor("#666666");
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total registros: {_items.Count}").FontSize(9).Bold().FontColor(_accentColor);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter().Text("No se encontraron registros para el período seleccionado.")
                .FontSize(11).FontColor("#999999").Italic();
            return;
        }

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(90);   // N° Reserva
                cols.RelativeColumn(2);    // Habitación
                cols.RelativeColumn(2);    // Huésped
                cols.ConstantColumn(90);   // Fecha Programada
                cols.ConstantColumn(90);   // Fecha Realizada
                cols.ConstantColumn(80);   // Diferencia
            });

            // Header
            table.Header(header =>
            {
                void HeaderCell(IContainer c, string text) =>
                    c.Background(_accentColor).Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                header.Cell().Element(c => HeaderCell(c, "N° Reserva"));
                header.Cell().Element(c => HeaderCell(c, "Habitación"));
                header.Cell().Element(c => HeaderCell(c, "Huésped"));
                header.Cell().Element(c => HeaderCell(c, "Fecha Programada"));
                header.Cell().Element(c => HeaderCell(c, "Fecha Realizada"));
                header.Cell().Element(c => HeaderCell(c, "Diferencia"));
            });

            foreach (var (item, index) in _items.Select((x, i) => (x, i)))
            {
                var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";
                var isLate = item.FechaRealizada > item.FechaProgramada;

                IContainer Cell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                table.Cell().Element(Cell).Text(item.NumeroReserva).FontSize(8);
                table.Cell().Element(Cell).Text(item.Habitacion).FontSize(8);
                table.Cell().Element(Cell).Text(item.Huesped).FontSize(8);
                table.Cell().Element(Cell).Text(item.FechaProgramada.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                table.Cell().Element(Cell).Text(item.FechaRealizada.ToString("dd/MM/yyyy HH:mm")).FontSize(8);

                var diferenciaTexto = FormatDiferencia(item.Diferencia);
                var diferenciaColor = isLate ? "#c62828" : "#2e7d32";
                var diferenciaPrefix = isLate ? "▲ +" : "▼ -";

                table.Cell().Element(Cell)
                    .Text($"{diferenciaPrefix}{diferenciaTexto}")
                    .FontSize(8).Bold().FontColor(diferenciaColor);
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

    private static string FormatDiferencia(TimeSpan diff)
    {
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}min";
        if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h {diff.Minutes}min";
        return $"{(int)diff.TotalDays}d {diff.Hours}h";
    }
}
