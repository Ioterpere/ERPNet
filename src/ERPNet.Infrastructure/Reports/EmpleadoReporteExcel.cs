using ClosedXML.Excel;
using ERPNet.Domain.Entities;

namespace ERPNet.Infrastructure.Reports;

public static class EmpleadoReporteExcel
{
    public static byte[] Generar(List<Empleado> empleados)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Empleados");

        // Headers
        var headers = new[] { "#", "Nombre", "Apellidos", "DNI", "Seccion", "Activo" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }

        // Data
        for (var i = 0; i < empleados.Count; i++)
        {
            var emp = empleados[i];
            var row = i + 2;

            ws.Cell(row, 1).Value = i + 1;
            ws.Cell(row, 2).Value = emp.Nombre;
            ws.Cell(row, 3).Value = emp.Apellidos;
            ws.Cell(row, 4).Value = emp.DNI.Value;
            ws.Cell(row, 5).Value = emp.Seccion?.Nombre ?? "—";
            ws.Cell(row, 6).Value = emp.Activo ? "Si" : "No";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }
}
