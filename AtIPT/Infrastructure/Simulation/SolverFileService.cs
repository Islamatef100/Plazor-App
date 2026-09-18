using System.Globalization;
using AtIPT.Application.Abstractions;
using AtIPT.Domain;

namespace AtIPT.Infrastructure.Simulation;

public sealed class SolverFileService : ISolverFileService
{
    public string ExeFolder { get; }

    public SolverFileService(string exeFolder)
    {
        ExeFolder = exeFolder;
        Directory.CreateDirectory(exeFolder);
    }

    private string PathOf(string name) => System.IO.Path.Combine(ExeFolder, name);

    public async Task WriteRawDataAsync(SimCase c)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Shut-in Time (hr) DeltaP, Field Data Derivative, Field Data");

        if (c.DifferenceVariables.Count >= 3 && c.DifferenceVariables[0].Values.Count > 0)
        {
            int n = c.DifferenceVariables[0].Values.Count;
            for (int i = 0; i < n; i++)
            {
                sb.Append(c.DifferenceVariables[0].Values[i].ToString("G10", CultureInfo.InvariantCulture)).Append(' ');
                sb.Append(c.DifferenceVariables[1].Values[i].ToString("G10", CultureInfo.InvariantCulture)).Append(' ');
                sb.Append(c.DifferenceVariables[2].Values[i].ToString("G10", CultureInfo.InvariantCulture));
                sb.AppendLine();
            }
        }
        else if (c.RawVariables.Count >= 3 && c.RawVariables[0].Values.Count > 0)
        {
            int n = c.RawVariables[0].Values.Count;
            for (int i = 0; i < n; i++)
            {
                sb.Append(c.RawVariables[1].Values[i].ToString("G10", CultureInfo.InvariantCulture)).Append(' ');
                sb.Append(c.RawVariables[0].Values[i].ToString("G10", CultureInfo.InvariantCulture)).Append(' ');
                sb.Append(c.RawVariables[2].Values[i].ToString("G10", CultureInfo.InvariantCulture));
                sb.AppendLine();
            }
        }

        await WriteTextSafeAsync("Raw_Data.txt", sb.ToString());
    }

    public async Task WriteModelParametersAsync(SimCase c)
    {
        var p = c.Parameters;
        var lines = new[]
        {
            p[0].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[7].Value.ToString("G10", CultureInfo.InvariantCulture),
            "0.01",
            p[9].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[4].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[10].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[5].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[6].Value.ToString("G10", CultureInfo.InvariantCulture),
            "0",
            p[18].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[11].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[12].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[13].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[14].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[1].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[15].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[16].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[8].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[19].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[20].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[22].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[23].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[24].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[2].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[3].Value.ToString("G10", CultureInfo.InvariantCulture),
            "0",
            p[32].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[21].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[27].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[28].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[29].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[30].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[31].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[25].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[26].Value.ToString("G10", CultureInfo.InvariantCulture),
            p[33].Value.ToString("G10", CultureInfo.InvariantCulture),
        };

        await WriteLinesSafeAsync("model_parameters.txt", lines);
    }

    public async Task WriteShrinkageFractureTypeAsync(SimCase c)
    {
        var lines = new[]
        {
            ((int)c.ShrinkageType).ToString(CultureInfo.InvariantCulture),
            ((int)c.FractureType + 1).ToString(CultureInfo.InvariantCulture)
        };
        await WriteLinesSafeAsync("sh_frac_type.txt", lines);
    }

    public async Task WriteBoundaryValuesAsync(SimCase c)
    {
        var p = c.AnalysisParameters;

        string Row(Func<int, double> pick) => string.Join("\t",
            pick(4).ToString("G10", CultureInfo.InvariantCulture),
            pick(0).ToString("G10", CultureInfo.InvariantCulture),
            pick(1).ToString("G10", CultureInfo.InvariantCulture),
            pick(8).ToString("G10", CultureInfo.InvariantCulture),
            pick(9).ToString("G10", CultureInfo.InvariantCulture),
            pick(3).ToString("G10", CultureInfo.InvariantCulture),
            pick(5).ToString("G10", CultureInfo.InvariantCulture),
            pick(6).ToString("G10", CultureInfo.InvariantCulture),
            pick(7).ToString("G10", CultureInfo.InvariantCulture),
            pick(12).ToString("G10", CultureInfo.InvariantCulture),
            pick(10).ToString("G10", CultureInfo.InvariantCulture));

        var lines = new[]
        {
            Row(i => p[i].Min),
            Row(i => p[i].Max)
        };

        await WriteLinesSafeAsync("boundary_values.txt", lines);
    }

    public async Task WriteUnitSystemAsync(bool metric)
    {
        await WriteTextSafeAsync("unitsystem.txt", metric ? "Metric" : "NotMetric");
    }

    public async Task WriteErrorToReachAsync(double? error)
    {
        var text = error.HasValue
            ? error.Value.ToString("0.0000", CultureInfo.InvariantCulture)
            : "0.0000";
        await WriteTextSafeAsync("error_to_reach.txt", text);
    }

    public async Task InitialiseWorkingFilesAsync()
    {
        // Kill any leftover solver processes first — they may still hold
        // a handle on Solver_Output.txt / Solver_Output_der.txt from a
        // previous session, which would cause the truncation below to
        // throw IOException (file in use).
        KillLeftoverSolvers();

        // Give Windows a moment to release the handles.
        await Task.Delay(300);

        foreach (var f in new[]
                 {
                     "Solver_Input.dat",
                     "Solver_Output.txt",
                     "Solver_Output_der.txt",
                     "GA_history.txt"
                 })
        {
            await WriteTextSafeAsync(f, string.Empty);
        }
    }

    // ------------------------------------------------------------------
    // Safe file writes — never throw if a file is temporarily locked.
    // Retry a few times, then give up silently. This makes startup
    // robust even if a Fortran process has not yet released its handle.
    // ------------------------------------------------------------------
    private async Task WriteTextSafeAsync(string fileName, string contents)
    {
        await WriteWithRetryAsync(PathOf(fileName), async path =>
        {
            await File.WriteAllTextAsync(path, contents);
        });
    }

    private async Task WriteLinesSafeAsync(string fileName, string[] lines)
    {
        await WriteWithRetryAsync(PathOf(fileName), async path =>
        {
            await File.WriteAllLinesAsync(path, lines);
        });
    }

    private static async Task WriteWithRetryAsync(string path, Func<string, Task> write)
    {
        const int maxAttempts = 5;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await write(path);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                await Task.Delay(200 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                await Task.Delay(200 * attempt);
            }
            catch (IOException)
            {
                // Give up — the app can still run; the solver will overwrite
                // these files when it starts.
                return;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
        }
    }

    private static void KillLeftoverSolvers()
    {
        string[] names =
        {
            "IPT_Dual_Finite_With_GA",        "IPT_Dual_Finite_Without_GA",
            "IPT_inFinite_Without_GA",        "IPT_inFinite_With_GA",
            "IPT_Single_Finite_With_GA",      "IPT_Single_Finite_Without_GA",
            "IPTSH_Finite_dual_With_GA",      "IPTSH_Finite_dual_Without_GA",
            "IPTSH_inFinite_With_GA",         "IPTSH_inFinite_Without_GA",
            "IPTSH_Single_Finite_With_GA",    "IPTSH_Single_Finite_Without_GA"
        };

        foreach (var p in System.Diagnostics.Process.GetProcesses())
        {
            if (names.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
            {
                try { p.Kill(); } catch { }
            }
        }
    }
}