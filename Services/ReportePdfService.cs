using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VeterinariaApp.Services
{
    // Genera un PDF con un encabezado y una tabla a partir de columnas y filas
    public class ReportePdfService
    {
        public byte[] GenerarReporte(string titulo, string? subtitulo, string[] columnas, List<string[]> filas)
        {
            return Document.Create(documento =>
            {
                documento.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Column(encabezado =>
                    {
                        encabezado.Item().Text("Clínica Veterinaria").FontSize(18).Bold().FontColor(Colors.Teal.Darken3);
                        encabezado.Item().Text(titulo).FontSize(14).SemiBold();
                        if (!string.IsNullOrWhiteSpace(subtitulo))
                            encabezado.Item().Text(subtitulo).FontColor(Colors.Grey.Darken1);
                        encabezado.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                        encabezado.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    page.Content().PaddingTop(10).Column(contenido =>
                    {
                        if (filas.Count == 0)
                        {
                            contenido.Item().Text("No hay registros para mostrar.").Italic();
                            return;
                        }

                        contenido.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c =>
                            {
                                foreach (var _ in columnas) c.RelativeColumn();
                            });

                            tabla.Header(h =>
                            {
                                foreach (var nombreColumna in columnas)
                                    h.Cell().Background(Colors.Teal.Darken3).Padding(5)
                                        .Text(nombreColumna).FontColor(Colors.White).Bold();
                            });

                            var i = 0;
                            foreach (var fila in filas)
                            {
                                var fondo = i++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                foreach (var celda in fila)
                                    tabla.Cell().Background(fondo).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                        .Padding(5).Text(celda);
                            }
                        });

                        contenido.Item().PaddingTop(10).AlignRight().Text($"Total de registros: {filas.Count}").SemiBold();
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.Span("Página ");
                        t.CurrentPageNumber();
                        t.Span(" de ");
                        t.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}
