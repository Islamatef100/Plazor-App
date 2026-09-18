using System.Globalization;
using AtIPT.Domain;

namespace AtIPT.Application.Services;

public static class ExportBuilder
{
    // ------------------------------------------------------------------
    // Full export — same sheets as the old project's
    // RibbonForm1.exportToExcel_ItemClick.
    // ------------------------------------------------------------------
    public static Dictionary<string, IReadOnlyList<IReadOnlyList<string>>> Build(SimCase c)
    {
        var sheets = new Dictionary<string, IReadOnlyList<IReadOnlyList<string>>>();

        var raw = new List<IReadOnlyList<string>>
        {
            new[] { "Shut-in Time (hr)", "DeltaP, Field Data", "Derivative, Field Data" }
        };
        double rate = c.Parameters.Count > 7 ? c.Parameters[7].Value : 1.0;
        int rawCount = c.DifferenceVariables.Count >= 3 ? c.DifferenceVariables[0].Values.Count : 0;
        for (int i = 0; i < rawCount; i++)
        {
            raw.Add(new[]
            {
                Num(c.DifferenceVariables[0].Values[i]),
                Num(rate * c.DifferenceVariables[1].Values[i]),
                Num(rate * c.DifferenceVariables[2].Values[i])
            });
        }
        sheets["Field Data"] = raw;

        for (int r = 0; r < c.Runs.Count; r++)
        {
            var run = c.Runs[r];
            var data = new List<IReadOnlyList<string>>
            {
                new[] { "Shut-in Time (hr)", "DeltaP", "Derivative" }
            };
            int n = run[0].Values.Count;
            for (int i = 0; i < n; i++)
            {
                data.Add(new[]
                {
                    Num(run[0].Values[i]),
                    Num(run[1].Values[i]),
                    Num(run[2].Values[i])
                });
            }
            sheets[$"Simulation Data for Run {r + 1}"] = data;

            var pars = new List<IReadOnlyList<string>> { new[] { "Parameter", "Value" } };
            if (r < c.RunParameters.Count)
            {
                var ps = c.RunParameters[r];
                for (int i = 0; i < ps.Length; i++)
                {
                    if (!ps[i].Enabled) continue;
                    pars.Add(new[] { ps[i].Name, Num(ps[i].Value) });
                }
            }
            sheets[$"Parameters of Run {r + 1}"] = pars;
        }

        return sheets;
    }

    // ------------------------------------------------------------------
    // User Guess export — single sheet with raw data, modeled data,
    // fracture info, parameters, and error. Matches the old project's
    // UserGuess.export_data_Table_to_Excel.
    //
    // The old export wrote everything into ONE sheet:
    //   Column A-C  : Raw/Field data (Time, DeltaP, Derivative)
    //   Column F-H  : Modeled data (Time, DeltaP, Derivative)
    //   Column I-K  : Fracture height, length, storage (constant)
    //   Column L-M  : Parameter names + values
    //   Column O    : Error (Residual Error)
    // ------------------------------------------------------------------
    public static Dictionary<string, IReadOnlyList<IReadOnlyList<string>>> BuildUserGuessExport(SimCase c)
    {
        var sheets = new Dictionary<string, IReadOnlyList<IReadOnlyList<string>>>();

        var raw = c.DifferenceVariables;
        var sim = c.SimulationResults;

        int rawCount = raw.Count >= 3 ? raw[0].Values.Count : 0;
        int simCount = sim.Count >= 3 ? sim[0].Values.Count : 0;
        int maxRows = Math.Max(rawCount, simCount);

        // Parameters — same ordering and skip rules as old project.
        var paramRows = new List<(string Name, string Value)>();
        var a = c.AnalysisParameters;
        var varsList = new[]
        {
            "Inner Zone Permeability (mD)",
            "Fracture Half Length (ft)",
            "Fracture Storage Constant (bbls/psi)",
            "Fracture Skin",
            "Dimensionless Fracture Conductivity",
            "Dimensionless Wellbore Storage Constant",
            "Injection Layer stress (psi)",
            "Containment Layer Stress(psi)",
            "Length Shrinkage Speed Parameter (Delpat)",
            "Mobility Front, Elliptical",
            "Mobility / Diffusivity Ratio",
            "Outer Zone Permeability (mD)"
        };

        // Values come from the same sources as the old project:
        //  idx 0: AnalysisParameters[4].Value
        //  idx 1: AnalysisParameters[0].Value
        //  idx 2: AnalysisParameters[2].Value (dim frac storage computed by solver)
        //  idx 3: AnalysisParameters[8].Value
        //  idx 4: AnalysisParameters[9].Value
        //  idx 5: (skipped)
        //  idx 6: AnalysisParameters[5].Value
        //  idx 7: AnalysisParameters[6].Value
        //  idx 8: AnalysisParameters[7].Value
        //  idx 9: AnalysisParameters[12].Value (Mobility Front)
        //  idx 10: AnalysisParameters[10].Value
        //  idx 11: outer zone perm — computed from inner/mobility/visc
        double outerPerm = 0;
        double injVisc = c.Parameters[6].Value;
        double formVisc = c.Parameters[25].Value;
        double inner = a[4].Value;
        double mobRatio = a[10].Value;
        if (c.ModelSelection == ModelSelection.FiniteSingleMobility) mobRatio = 1;
        if (mobRatio != 0 && injVisc != 0) outerPerm = (inner / mobRatio) * (formVisc / injVisc);

        paramRows.Add((varsList[0], Num(inner)));
        paramRows.Add((varsList[1], Num(a[0].Value)));
        paramRows.Add((varsList[2], Num(a[2].Value)));
        if (c.ModelSelection != ModelSelection.InfiniteConductivity)
        {
            paramRows.Add((varsList[3], Num(a[8].Value)));
            paramRows.Add((varsList[4], Num(a[9].Value)));
        }
        if (c.ShrinkageType != ShrinkageType.None)
        {
            paramRows.Add((varsList[6], Num(a[5].Value)));
            paramRows.Add((varsList[7], Num(a[6].Value)));
            paramRows.Add((varsList[8], Num(a[7].Value)));
        }
        if (c.ModelSelection != ModelSelection.FiniteSingleMobility)
        {
            paramRows.Add((varsList[9], Num(a[12].Value)));
            paramRows.Add((varsList[10], Num(a[10].Value)));
        }
        paramRows.Add((varsList[11], Num(outerPerm)));

        // Build the sheet as a rectangular grid of strings.
        // Column index is 0-based here; it maps to Excel A=0, B=1, ...
        int totalCols = 15; // A..O
        int totalRows = maxRows + 2;

        var grid = new string[totalRows][];
        for (int r = 0; r < totalRows; r++)
        {
            grid[r] = new string[totalCols];
            for (int cc = 0; cc < totalCols; cc++) grid[r][cc] = "";
        }

        // Row 0 — headers
        grid[0][0] = "Time";
        grid[0][1] = "Field Delta P";
        grid[0][2] = "Field Dervative";
        grid[0][5] = "Time";
        grid[0][6] = "Modeled Delta P";
        grid[0][7] = "Modeled Dervative";
        grid[0][8] = "Fracture height";
        grid[0][9] = "Fracture length";
        grid[0][10] = "Fracture Storage";
        grid[0][11] = "Parameter";
        grid[0][12] = "Value";
        grid[0][14] = "Error";

        // Row 1 — error value
        grid[1][14] = Num(c.TestGuessFitness ?? 0);

        // Parameters — starting at row 1 (0-based index 1) column L
        for (int i = 0; i < paramRows.Count && i + 1 < totalRows; i++)
        {
            grid[i + 1][11] = paramRows[i].Name;
            grid[i + 1][12] = paramRows[i].Value;
        }

        // Raw data starting at row 2 (0-based). Column A-C.
        for (int i = 0; i < rawCount && i + 2 < totalRows; i++)
        {
            grid[i + 2][0] = Num(raw[0].Values[i]);
            grid[i + 2][1] = Num(Math.Abs(raw[1].Values[i]));
            grid[i + 2][2] = Num(raw[2].Values[i]);
        }

        // Modeled data starting at row 2 (0-based). Column F-K.
        // Fracture height/length/storage are constant strings per row.
        string fracHeight = Num(c.Parameters[9].Value);
        string fracLength = Num(c.Parameters[23].Value);
        string fracStorage = Num(c.Parameters[21].Value);

        for (int i = 0; i < simCount && i + 2 < totalRows; i++)
        {
            grid[i + 2][5] = Num(sim[0].Values[i]);
            grid[i + 2][6] = Num(sim[1].Values[i]);
            grid[i + 2][7] = Num(sim[2].Values[i]);
            grid[i + 2][8] = fracHeight;
            grid[i + 2][9] = fracLength;
            grid[i + 2][10] = fracStorage;
        }

        // Convert to the sheet dictionary format used by the Excel service.
        var sheetRows = new List<IReadOnlyList<string>>();
        foreach (var row in grid) sheetRows.Add(row);

        sheets["User Guess Data "] = sheetRows;
        return sheets;
    }

    private static string Num(double v)
        => v.ToString("G10", CultureInfo.InvariantCulture);
}