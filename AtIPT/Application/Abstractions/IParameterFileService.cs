using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

/// <summary>
/// Read/write a Model Parameters text file in the same labeled format
/// used by FormParamMain.setParm / saveToFile_Click in the old project.
/// </summary>
public interface IParameterFileService
{
    /// <summary>
    /// Reads a parameter file and returns a dictionary of
    /// SimCase.Parameters index → value.
    /// </summary>
    Task<Dictionary<int, double>> ReadAsync(Stream stream, CancellationToken ct = default);

    /// <summary>
    /// Serialises the current SimCase parameters to the old text format.
    /// </summary>
    string Write(SimCase simCase);
}