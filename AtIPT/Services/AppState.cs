using AtIPT.Application.Abstractions;
using AtIPT.Domain;
using AtIPT.Helpers;

namespace AtIPT.Services;

public enum PickTarget
{
    None,
    ShutInPoint,
    EndPoint,
    FscFirstPoint,
    PermeabilityPoint,
    MobilityPoint,
    FhlFromFsc
}

public sealed class AppState
{
    private readonly IProjectRepository _repo;

    public AppState(IProjectRepository repo)
    {
        _repo = repo;
        Case.ParametersInit();
        Case.AnalysisParametersInit();
    }

    public SimCase Case { get; private set; } = new();
    public IReadOnlyList<Variable> ImportedVariables { get; set; } = Array.Empty<Variable>();
    public IReadOnlyList<ProjectNode> Projects { get; set; } = Array.Empty<ProjectNode>();

    public string? CurrentProjectName { get; set; }
    public string? CurrentProjectFolder { get; set; }
    public string? SelectedFileName { get; set; }
    public string? CurrentInputFileName { get; set; }
    public string? CurrentInputFilePath { get; set; }

    public string? BusyMessage { get; set; }
    public bool IsBusy => BusyMessage is not null;

    public CancellationTokenSource? SolverCts { get; set; }

    public PickTarget ActivePickTarget { get; set; } = PickTarget.None;
    public List<(double X, double Y)> PickedPoints { get; } = new();

    public void BeginPick(PickTarget target)
    {
        ActivePickTarget = target;
        PickedPoints.Clear();
        NotifyChanged();
    }

    public void CancelPick()
    {
        ActivePickTarget = PickTarget.None;
        PickedPoints.Clear();
        NotifyChanged();
    }

    public HashSet<string> SetBoundsEnd { get; } = new(StringComparer.Ordinal);

    private static int NormaliseShrinkage(ShrinkageType s)
        => s == ShrinkageType.None ? 0 : 1;

    public bool IsBoundsSetForCurrent() =>
        SetBoundsEnd.Contains($"{Case.ModelSelection}_{NormaliseShrinkage(Case.ShrinkageType)}");

    public void MarkBoundsSetForCurrent() =>
        SetBoundsEnd.Add($"{Case.ModelSelection}_{NormaliseShrinkage(Case.ShrinkageType)}");

    public bool CanOpen { get; set; } = true;
    public bool CanImport { get; set; } = true;
    public bool CanSave { get; set; } = false;
    public bool CanSaveAs { get; set; } = false;
    public bool CanExit { get; set; } = true;
    public bool CanSelectInputs { get; set; } = false;
    public bool CanModelParameters { get; set; } = false;
    public bool CanSimulationSetup { get; set; } = false;
    public bool CanRawData { get; set; } = false;
    public bool CanDiffPressure { get; set; } = false;
    public bool CanBoundCurves { get; set; } = false;
    public bool CanUserGuessMatch { get; set; } = false;
    public bool CanAutoRunSolutions { get; set; } = false;
    public bool CanLoadHistory { get; set; } = true;

    public event Action? OnChange;

    public void ResetCase()
    {
        Case = new SimCase();
        Case.ParametersInit();
        Case.AnalysisParametersInit();

        ImportedVariables = Array.Empty<Variable>();
        SelectedFileName = null;
        CurrentInputFileName = null;
        CurrentInputFilePath = null;
        SetBoundsEnd.Clear();
        ActivePickTarget = PickTarget.None;
        PickedPoints.Clear();

        CanSave = CanSaveAs = CanSelectInputs = CanModelParameters = false;
        CanSimulationSetup = CanRawData = CanDiffPressure = false;
        CanBoundCurves = CanUserGuessMatch = CanAutoRunSolutions = false;

        NotifyChanged();
    }

    public void ClearImportState()
    {
        ImportedVariables = Array.Empty<Variable>();
        SelectedFileName = null;
        CurrentInputFileName = null;
        CurrentInputFilePath = null;

        Case.ImportedVariables.Clear();
        Case.RawVariables.Clear();
        Case.OriginalRawData.Clear();
        Case.DifferenceVariables.Clear();
        Case.InitialGuessMin.Clear();
        Case.InitialGuessMax.Clear();
        Case.SimulationResults.Clear();
        Case.Runs.Clear();
        Case.RunParameters.Clear();
        Case.SolutionFitnesses.Clear();

        ActivePickTarget = PickTarget.None;
        PickedPoints.Clear();
        SetBoundsEnd.Clear();

        CanSave = CanSaveAs = CanSelectInputs = CanModelParameters = false;
        CanSimulationSetup = CanRawData = CanDiffPressure = false;
        CanBoundCurves = CanUserGuessMatch = CanAutoRunSolutions = false;

        NotifyChanged();
    }

    public void EnableAfterImport() { CanSave = CanSaveAs = CanSelectInputs = true; NotifyChanged(); }
    public void EnableAfterSelectInputs() { CanModelParameters = CanRawData = true; NotifyChanged(); }
    public void EnableAfterModelParameters() { CanDiffPressure = CanSimulationSetup = true; NotifyChanged(); }
    public void EnableAfterSetBounds() { CanBoundCurves = true; NotifyChanged(); }
    public void EnableAfterTestGuess() { CanUserGuessMatch = true; NotifyChanged(); }
    public void EnableAfterAutoRun() { CanAutoRunSolutions = true; NotifyChanged(); }

    public void ApplyEnablementFromLoadedCase()
    {
        bool hasImported = Case.ImportedVariables.Count > 0 || Case.RawVariables.Count > 0;
        bool hasRaw = Case.RawVariables.Count > 0;
        bool hasDiff = Case.DifferenceVariables.Count > 0;
        bool hasBounds = Case.InitialGuessMin.Count > 0 || Case.InitialGuessMax.Count > 0;
        bool hasGuess = Case.SimulationResults.Count > 0;
        bool hasRuns = Case.Runs.Count > 0;

        CanSave = CanSaveAs = CanSelectInputs = CanModelParameters = hasImported;
        CanRawData = hasRaw;
        CanDiffPressure = hasDiff;
        CanSimulationSetup = hasDiff;
        CanBoundCurves = hasBounds;
        CanUserGuessMatch = hasGuess;
        CanAutoRunSolutions = hasRuns;

        if (hasBounds) MarkBoundsSetForCurrent();

        NotifyChanged();
    }

    /// <summary>
    /// Analysis Parameters defaults — port of AnalysisForm.load() plus the
    /// two extra values the old project let the user type manually
    /// (Mobility Ratio and Diffusivity Ratio).
    ///
    /// The old project's Simulation Setup screen had two text boxes at the
    /// top of the Parameters section. Users typed the Mobility Ratio and
    /// Diffusivity Ratio there before clicking Test Guess. Without those
    /// values the Fortran solver receives zeta = 0 and crashes inside the
    /// Mathieu function with "bad arguments in bessik". We reproduce the
    /// same defaults here so the solver works even if the user does not
    /// touch the two new text boxes.
    /// </summary>
    public void ApplyAnalysisParameterDefaults()
    {
        var m = Case.Parameters;
        var a = Case.AnalysisParameters;

        if (m.Count < 34 || a.Count < 14) return;

        a[0].Min = Math.Round(m[23].Value * 0.8, 4);
        a[0].Max = Math.Round(m[23].Value * 1.2, 4);

        double injThk = m[9].Value;
        double poisson = m[12].Value;
        double young = m[13].Value;
        int ftype = (int)Case.FractureType;

        double fscMin = MathHelpers.GetFracStorage(a[0].Min, injThk, poisson, young, ftype);
        double fscMax = MathHelpers.GetFracStorage(a[0].Max, injThk, poisson, young, ftype);
        a[1].Min = Math.Round(fscMin, 4);
        a[1].Max = Math.Round(fscMax, 4);

        a[2].Min = 0;
        a[2].Max = 0;

        double injComp = m[5].Value;
        double wbVolume = m[4].Value;
        double poros = m[10].Value;
        double totalComp = m[14].Value;

        if (poros != 0 && totalComp != 0 && injThk != 0 && wbVolume != 0)
        {
            double denomMin = 2 * Math.PI * Math.Pow(a[0].Min, 2) * poros * totalComp * injThk;
            double denomMax = 2 * Math.PI * Math.Pow(a[0].Max, 2) * poros * totalComp * injThk;

            a[3].Min = denomMin != 0 ? Math.Round((injComp * wbVolume) / denomMin, 6) : 0;
            a[3].Max = denomMax != 0 ? Math.Round((injComp * wbVolume) / denomMax, 6) : 0;
        }

        a[4].Min = Math.Round(m[18].Value * 0.8, 4);
        a[4].Max = Math.Round(m[18].Value * 1.2, 4);

        a[5].Min = m[30].Value;
        a[5].Max = m[30].Value;

        a[6].Min = m[31].Value;
        a[6].Max = m[31].Value;

        a[7].Min = 0.1;
        a[7].Max = 3;

        a[8].Min = 0;
        a[8].Max = 1;

        a[9].Min = 0.1;
        a[9].Max = 10;

        a[10].Min = 0.1;
        a[10].Max = 0.9;

        a[11].Min = 0.1;
        a[11].Max = 0.9;

        a[12].Min = 0.1;
        a[12].Max = 5;

        // User Guess defaults. These are the values the old project's
        // text boxes held when the user started a fresh project.
        // The most important ones: Mobility Ratio and Diffusivity Ratio
        // must be non-zero or the Fortran Mathieu function crashes.
        if (a[0].Value == 0) a[0].Value = a[0].Min > 0 ? a[0].Min : 100;
        if (a[4].Value == 0) a[4].Value = a[4].Min > 0 ? a[4].Min : 1;
        if (a[10].Value == 0) a[10].Value = 0.5;
        if (a[11].Value == 0) a[11].Value = 0.5;
        if (a[8].Value == 0) a[8].Value = 0;
        if (a[9].Value == 0) a[9].Value = 1;
    }

    public async Task SaveCurrentAsync()
    {
        if (string.IsNullOrEmpty(CurrentProjectFolder)) return;
        await _repo.SaveCaseAsync(CurrentProjectFolder, Case, CurrentInputFilePath);
    }

    public async Task SaveAsNewAsync()
    {
        if (string.IsNullOrEmpty(CurrentProjectName)) return;
        var newName = $"{CurrentProjectName}_copy_{DateTime.Now:yyyyMMdd_HHmmss}";
        CurrentProjectFolder = await _repo.CreateProjectAsync(newName);
        CurrentProjectName = newName;
        await _repo.SaveCaseAsync(CurrentProjectFolder, Case, CurrentInputFilePath);
        Projects = await _repo.ListProjectsAsync();
        NotifyChanged();
    }

    public void SetBusy(string? message) { BusyMessage = message; NotifyChanged(); }
    public void NotifyChanged() => OnChange?.Invoke();
}