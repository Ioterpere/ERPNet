using ERPNet.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERPNet.Infrastructure.Reports;

public class EmpleadoReportePdf(List<Empleado> empleados) : IDocument
{
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.PaddingBottom(10).Column(column =>
        {
            column.Item().Text("Listado de Empleados")
                .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);

            column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);   // #
                columns.RelativeColumn(2);    // Nombre
                columns.RelativeColumn(2);    // Apellidos
                columns.RelativeColumn(1.5f); // DNI
                columns.RelativeColumn(2);    // Seccion
                columns.ConstantColumn(50);   // Activo
            });

            table.Header(header =>
            {
                var headerStyle = TextStyle.Default.Bold().FontColor(Colors.White);

                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("#").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Nombre").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Apellidos").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("DNI").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Seccion").Style(headerStyle);
                header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Activo").Style(headerStyle);
            });

            for (var i = 0; i < empleados.Count; i++)
            {
                var emp = empleados[i];
                var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                table.Cell().Background(bg).Padding(5).Text((i + 1).ToString());
                table.Cell().Background(bg).Padding(5).Text(emp.Nombre);
                table.Cell().Background(bg).Padding(5).Text(emp.Apellidos);
                table.Cell().Background(bg).Padding(5).Text(emp.DNI);
                table.Cell().Background(bg).Padding(5).Text(emp.Seccion?.Nombre ?? "—");
                table.Cell().Background(bg).Padding(5).Text(emp.Activo ? "Si" : "No");
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Pagina ");
            text.CurrentPageNumber();
            text.Span(" de ");
            text.TotalPages();
        });
    }
}
