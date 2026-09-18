using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

public interface ISimulationRunner
{
    /// <summary>Writes Solver_Input.dat, runs the given .exe, waits for exit, returns result.</summary>
    Task<SimulationResult> RunAsync(string exeName, string solverInputContents, CancellationToken ct = default);

    /// <summary>Kills every running Fortran solver process (used on "Stop").</summary>
    Task KillAllAsync();

    /// <summary>Path of Solver_Output_der.txt inside the exe folder.</summary>
    string OutputDerPath { get; }

    /// <summary>Path of Solver_Output.txt inside the exe folder.</summary>
    string OutputPath { get; }
}