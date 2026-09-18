using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

public enum RunMode { Min, Max, User }

public interface IAnalysisService
{
    string BuildSolverInput(SimCase simCase, RunMode mode);
    Task<SimulationResult> RunSolverAsync(SimCase simCase, bool withGA, CancellationToken ct = default);
    string? ValidateBounds(SimCase simCase, double shutInPressure);
    string? ValidateUserGuess(SimCase simCase, double shutInPressure);

    /// <summary>Port of AnalysisForm.chkMobility().</summary>
    string? CheckMobility(SimCase simCase);
}