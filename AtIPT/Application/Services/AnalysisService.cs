using AtIPT.Application.Abstractions;
using AtIPT.Domain;

namespace AtIPT.Application.Services;

public sealed class AnalysisService : IAnalysisService
{
    private readonly ISimulationRunner _runner;

    public AnalysisService(ISimulationRunner runner) => _runner = runner;

    public string BuildSolverInput(SimCase simCase, RunMode mode)
        => SolverInputBuilder.Build(simCase, mode);

    public async Task<SimulationResult> RunSolverAsync(SimCase simCase, bool withGA, CancellationToken ct = default)
    {
        var exeName = ResolveExecutable(simCase, withGA);
        var contents = SolverInputBuilder.Build(simCase, RunMode.User);
        return await _runner.RunAsync(exeName, contents, ct);
    }

    public string? ValidateBounds(SimCase simCase, double shutInPressure)
        => ParameterValidationService.ValidateBounds(simCase, shutInPressure);

    public string? ValidateUserGuess(SimCase simCase, double shutInPressure)
        => ParameterValidationService.ValidateUserGuess(simCase, shutInPressure);

    public string? CheckMobility(SimCase simCase)
    {
        // Direct port of AnalysisForm.chkMobility().
        if (simCase.AnalysisParameters[12].Min == 0
            && simCase.ModelSelection == ModelSelection.FiniteDualMobility
            && simCase.AnalysisParameters[10].Min != 1)
        {
            return "Mobility Front must be a positive number.  "
                 + "If Mobility Front = 0, please either use the Single Mobility option "
                 + "or set “Mobility Ratio” equal to 1”";
        }
        return null;
    }

    private static string ResolveExecutable(SimCase c, bool withGA)
    {
        if (c.ShrinkageType != ShrinkageType.None)
        {
            return c.ModelSelection switch
            {
                ModelSelection.InfiniteConductivity => withGA ? "IPTSH_inFinite_With_GA.exe" : "IPTSH_inFinite_Without_GA.exe",
                ModelSelection.FiniteSingleMobility => withGA ? "IPTSH_Single_Finite_With_GA.exe" : "IPTSH_Single_Finite_Without_GA.exe",
                ModelSelection.FiniteDualMobility => withGA ? "IPTSH_Finite_dual_With_GA.exe" : "IPTSH_Finite_dual_Without_GA.exe",
                _ => "IPTSH_inFinite_With_GA.exe"
            };
        }
        return c.ModelSelection switch
        {
            ModelSelection.InfiniteConductivity => withGA ? "IPT_inFinite_With_GA.exe" : "IPT_inFinite_Without_GA.exe",
            ModelSelection.FiniteSingleMobility => withGA ? "IPT_Single_Finite_With_GA.exe" : "IPT_Single_Finite_Without_GA.exe",
            ModelSelection.FiniteDualMobility => withGA ? "IPT_Dual_Finite_With_GA.exe" : "IPT_Dual_Finite_Without_GA.exe",
            _ => "IPT_inFinite_With_GA.exe"
        };
    }
}