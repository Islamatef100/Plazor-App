using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

/// <summary>
/// Writes every text file the solver workflow expects on disk.
/// Mirrors the disk side-effects of the old FormParamMain and AnalysisForm.
/// </summary>
public interface ISolverFileService
{
    string ExeFolder { get; }

    /// <summary>FormParamMain.rawData_write() — writes Raw_Data.txt from simCase.RawVariables.</summary>
    Task WriteRawDataAsync(SimCase c);

    /// <summary>FormParamMain.write_model_parameters() — writes model_parameters.txt.</summary>
    Task WriteModelParametersAsync(SimCase c);

    /// <summary>AnalysisForm.setSelectedComboToFile() — writes sh_frac_type.txt.</summary>
    Task WriteShrinkageFractureTypeAsync(SimCase c);

    /// <summary>AnalysisForm.saveToFile() — writes boundary_values.txt (min row then max row).</summary>
    Task WriteBoundaryValuesAsync(SimCase c);

    /// <summary>AnalysisForm.save_unit_system() — writes unitsystem.txt.</summary>
    Task WriteUnitSystemAsync(bool metric);

    /// <summary>FormInputBox.button1_Click() — writes error_to_reach.txt.</summary>
    Task WriteErrorToReachAsync(double? error);

    /// <summary>RibbonForm1_Load — truncate solver working files on startup.</summary>
    Task InitialiseWorkingFilesAsync();
}