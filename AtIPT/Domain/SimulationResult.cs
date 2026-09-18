namespace AtIPT.Domain;

/// <summary>
/// Outcome of a single Fortran solver invocation. Mirrors the old convention where
/// success meant "Solver_Output_der.txt was produced and its first line was not
/// 'bad arguments'".
/// </summary>
public sealed record SimulationResult(
    bool Success,
    string? OutputPath,
    string? ErrorMessage)
{
    public static SimulationResult Ok(string path) => new(true, path, null);
    public static SimulationResult Fail(string reason) => new(false, null, reason);
}