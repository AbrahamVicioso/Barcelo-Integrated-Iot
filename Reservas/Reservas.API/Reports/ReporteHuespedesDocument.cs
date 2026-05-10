using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Reservas.Application.DTOs.Reports;

namespace Reservas.API.Reports;

public class ReporteHuespedesDocument : IDocument
{
    private readonly List<ReporteHuespedItemDto> _items;
    private readonly DateTime? _fechaInicio;
    private readonly DateTime? _fechaFin;
    private readonly bool? _soloVip;

    private static readonly byte[]? _logo = LoadLogo();
    private static byte[]? LoadLogo()
    {
        using var stream = typeof(ReporteHuespedesDocument).Assembly
            .GetManifestResourceStream("Reservas.API.Reports.smartstay-icon.jpg");
        if (stream is null) return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public ReporteHuespedesDocument(List<ReporteHuespedItemDto> items, DateTime? fechaInicio, DateTime? fechaFin, bool? soloVip)
    {
        _items = items;
        _fechaInicio = fechaInicio;
        _fechaFin = fechaFin;
        _soloVip = soloVip;
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
        container.BorderBottom(2).BorderColor("#006064").PaddingBottom(8).Row(row =>
        {
            if (_logo != null)
                row.ConstantItem(55).Height(44).Image(_logo).FitHeight();
            row.RelativeItem().PaddingLeft(_logo != null ? 10 : 0).Column(c =>
            {
                c.Item().Text("SmartStay").FontSize(18).Bold().FontColor("#006064");
                c.Item().Text("Reporte de Huéspedes Registrados").FontSize(13).FontColor("#444444");
                if (_soloVip == true)
                    c.Item().PaddingTop(2).Text("★ Solo huéspedes VIP").FontSize(9).FontColor("#f9a825").Italic();
            });
            row.ConstantItem(220).AlignRight().Column(c =>
            {
                var periodo = _fechaInicio.HasValue
                    ? $"Registro: {_fechaInicio:dd/MM/yyyy} — {_fechaFin:dd/MM/yyyy}"
                    : "Período: Todos los registros";
                c.Item().Text(periodo).FontSize(9).FontColor("#666666");
                c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor("#999999");
                c.Item().PaddingTop(4).Text($"Total: {_items.Count} | VIP: {_items.Count(x => x.EsVip)}").FontSize(9).Bold().FontColor("#006064");
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        if (_items.Count == 0)
        {
            container.PaddingTop(40).AlignCenter()
                .Text("No se encontraron huéspedes con los filtros aplicados.")
                .FontSize(11).FontColor("#999999").Italic();
            return;
        }

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(2.5f);// Nombre
                cols.RelativeColumn(1.2f);// Tipo Doc
                cols.RelativeColumn(1.5f);// N° Doc
                cols.RelativeColumn(1.5f);// Nacionalidad
                cols.ConstantColumn(40);  // VIP
                cols.RelativeColumn(2);   // Email
                cols.ConstantColumn(85);  // Fecha Registro
            });

            table.Header(header =>
            {
                void HeaderCell(IContainer c, string text) =>
                    c.Background("#006064").Padding(6).Text(text).FontColor(Colors.White).FontSize(8).Bold();

                header.Cell().Element(c => HeaderCell(c, "Nombre Completo"));
                header.Cell().Element(c => HeaderCell(c, "Tipo Doc."));
                header.Cell().Element(c => HeaderCell(c, "N° Documento"));
                header.Cell().Element(c => HeaderCell(c, "Nacionalidad"));
                header.Cell().Element(c => HeaderCell(c, "VIP"));
                header.Cell().Element(c => HeaderCell(c, "Email"));
                header.Cell().Element(c => HeaderCell(c, "Fecha Registro"));
            });

            foreach (var (item, index) in _items.Select((x, i) => (x, i)))
            {
                var bg = index % 2 == 0 ? "#ffffff" : "#f5f7fa";
                IContainer Cell(IContainer c) =>
                    c.Background(bg).BorderBottom(1).BorderColor("#e8ecf0").Padding(5);

                table.Cell().Element(Cell).Text(item.NombreCompleto).FontSize(8);
                table.Cell().Element(Cell).Text(item.TipoDocumento).FontSize(8);
                table.Cell().Element(Cell).Text(item.NumeroDocumento).FontSize(8);
                table.Cell().Element(Cell).Text(item.Nacionalidad).FontSize(8);
                table.Cell().Element(Cell).AlignCenter()
                    .Text(item.EsVip ? "★" : "-").FontSize(9)
                    .FontColor(item.EsVip ? "#f9a825" : "#aaaaaa");
                table.Cell().Element(Cell).Text(item.Email).FontSize(7.5f);
                table.Cell().Element(Cell).Text(item.FechaRegistro.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
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
