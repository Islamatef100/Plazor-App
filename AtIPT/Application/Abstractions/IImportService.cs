using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

public interface IImportService
{
    IReadOnlyList<Variable> ReadFile(string filePath);
    IReadOnlyList<string> GetColumnNames(string filePath);
}