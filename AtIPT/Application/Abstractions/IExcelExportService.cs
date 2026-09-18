namespace AtIPT.Application.Abstractions;

public interface IExcelExportService
{
    /// <summary>Creates an .xlsx in memory. Sheets are keyed by name, values are rows of strings.</summary>
    byte[] CreateWorkbook(IReadOnlyDictionary<string, IReadOnlyList<IReadOnlyList<string>>> sheets);
}