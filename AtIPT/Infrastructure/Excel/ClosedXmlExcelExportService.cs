using AtIPT.Application.Abstractions;
using ClosedXML.Excel;

namespace AtIPT.Infrastructure.Excel;

public sealed class ClosedXmlExcelExportService : IExcelExportService
{
    public byte[] CreateWorkbook(IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<string>>> sheets)
    {
        using var wb = new XLWorkbook();

        foreach (var (name, rows) in sheets)
        {
            var ws = wb.Worksheets.Add(SanitiseSheetName(name));

            for (int r = 0; r < rows.Count; r++)
                for (int c = 0; c < rows[r].Count; c++)
                    ws.Cell(r + 1, c + 1).Value = rows[r][c];

            // formatSheet equivalent:
            ws.Columns().AdjustToContents();
            ws.RangeUsed()?.Style.NumberFormat.SetFormat("0.000000");
            ws.SheetView.FreezeRows(1);
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static string SanitiseSheetName(string n)
    {
        foreach (var ch in new[] { ':', '\\', '/', '?', '*', '[', ']' })
            n = n.Replace(ch, '_');
        return n.Length > 31 ? n[..31] : n;
    }
}