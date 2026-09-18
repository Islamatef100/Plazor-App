using System.Diagnostics;
using System.Text;
using AtIPT.Application.Abstractions;
using AtIPT.Domain;
using Microsoft.Extensions.Logging;

namespace AtIPT.Infrastructure.Simulation;

public sealed class FortranProcessRunner : ISimulationRunner
{
    private readonly string _exeDir;
    private readonly ILogger<FortranProcessRunner> _log;
    private static readonly Random Rnd = new();

    private static readonly TimeSpan SolverTimeout = TimeSpan.FromMinutes(15);

    private static readonly string[] SolverNames =
    {
        "IPT_Dual_Finite_With_GA",        "IPT_Dual_Finite_Without_GA",
        "IPT_inFinite_Without_GA",        "IPT_inFinite_With_GA",
        "IPT_Single_Finite_With_GA",      "IPT_Single_Finite_Without_GA",
        "IPTSH_Finite_dual_With_GA",      "IPTSH_Finite_dual_Without_GA",
        "IPTSH_inFinite_With_GA",         "IPTSH_inFinite_Without_GA",
        "IPTSH_Single_Finite_With_GA",    "IPTSH_Single_Finite_Without_GA"
    };

    public FortranProcessRunner(string exeDir, ILogger<FortranProcessRunner> logger)
    {
        _exeDir = exeDir;
        _log = logger;
        Directory.CreateDirectory(_exeDir);
    }

    public string OutputDerPath => Path.Combine(_exeDir, "Solver_Output_der.txt");
    public string OutputPath => Path.Combine(_exeDir, "Solver_Output.txt");

    public async Task<SimulationResult> RunAsync(string exeName, string solverInputContents, CancellationToken ct = default)
    {
        var exePath = Path.Combine(_exeDir, exeName);
        if (!File.Exists(exePath))
            return SimulationResult.Fail($"Executable not found: {exePath}");

        var inputPath = Path.Combine(_exeDir, "Solver_Input.dat");
        var derPath = OutputDerPath;
        var outPath = OutputPath;

        // Write the input file.
        await File.WriteAllTextAsync(inputPath, solverInputContents, CancellationToken.None);
        _log.LogInformation("Running {Exe}. Solver_Input.dat:\n{Input}", exeName, solverInputContents);

        // Truncate the outputs so we never read stale data.
        try { await File.WriteAllTextAsync(derPath, string.Empty, CancellationToken.None); } catch { }
        try { await File.WriteAllTextAsync(outPath, string.Empty, CancellationToken.None); } catch { }

        try { Environment.CurrentDirectory = _exeDir; } catch { }

        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = GenerateKey(),
            WorkingDirectory = _exeDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process? proc = null;
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        try
        {
            proc = Process.Start(psi);
            if (proc is null)
                return SimulationResult.Fail($"Failed to start \"{exeName}\"");

            // Close stdin so the exe sees EOF immediately.
            try { proc.StandardInput.Close(); } catch { }

            // Standard .NET async-output pattern. Events fill the buffers
            // while WaitForExit blocks — no deadlock possible.
            proc.OutputDataReceived += (s, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            // Wait for exit with timeout. Cancellation is checked via the token.
            bool exited;
            var start = DateTime.UtcNow;
            while (true)
            {
                if (proc.HasExited) { exited = true; break; }

                if (ct.IsCancellationRequested)
                {
                    try { proc.Kill(); } catch { }
                    try { proc.WaitForExit(3000); } catch { }
                    return SimulationResult.Fail("Cancelled by user.");
                }

                if ((DateTime.UtcNow - start) > SolverTimeout)
                {
                    try { proc.Kill(); } catch { }
                    try { proc.WaitForExit(3000); } catch { }
                    return SimulationResult.Fail(
                        $"Solver \"{exeName}\" did not finish within {SolverTimeout.TotalMinutes:0} minutes.");
                }

                await Task.Delay(200);
            }

            // Drain remaining async reads.
            try { proc.WaitForExit(); } catch { }

            _log.LogInformation("{Exe} exited with code {Code}. stdout: {Out}, stderr: {Err}",
                exeName, proc.ExitCode, stdout.ToString(), stderr.ToString());

            var ok = await Helpers.FileHelpers.IsSolverOutputValidAsync(derPath);
            if (ok) return SimulationResult.Ok(derPath);

            // Build a detailed diagnostic message.
            string stdoutPreview = stdout.ToString().Trim();
            if (stdoutPreview.Length > 300) stdoutPreview = stdoutPreview[..300] + " ...";
            if (stdoutPreview.Length == 0) stdoutPreview = "(empty)";

            string stderrPreview = stderr.ToString().Trim();
            if (stderrPreview.Length > 300) stderrPreview = stderrPreview[..300] + " ...";
            if (stderrPreview.Length == 0) stderrPreview = "(empty)";

            string derPreview = "(empty)";
            try
            {
                if (File.Exists(derPath))
                {
                    var t = (await File.ReadAllTextAsync(derPath)).Trim();
                    if (t.Length > 200) t = t[..200] + " ...";
                    if (t.Length > 0) derPreview = t;
                }
            }
            catch { }

            string outPreview = "(empty)";
            try
            {
                if (File.Exists(outPath))
                {
                    var t = (await File.ReadAllTextAsync(outPath)).Trim();
                    if (t.Length > 200) t = t[..200] + " ...";
                    if (t.Length > 0) outPreview = t;
                }
            }
            catch { }

            return SimulationResult.Fail(
                $"Solver \"{exeName}\" exited with code {proc.ExitCode} but produced no usable data. " +
                $"stdout: \"{stdoutPreview}\". " +
                $"stderr: \"{stderrPreview}\". " +
                $"Solver_Output_der.txt: \"{derPreview}\". " +
                $"Solver_Output.txt: \"{outPreview}\".");
        }
        catch (Exception ex)
        {
            try { if (proc is { HasExited: false }) proc.Kill(); } catch { }
            _log.LogError(ex, "Failed to run {Exe}", exeName);
            return SimulationResult.Fail($"Failed to launch \"{exeName}\": {ex.Message}");
        }
    }

    public Task KillAllAsync()
    {
        foreach (var p in Process.GetProcesses())
        {
            if (SolverNames.Contains(p.ProcessName, StringComparer.OrdinalIgnoreCase))
            {
                try { p.Kill(); } catch { }
            }
        }
        return Task.CompletedTask;
    }

    private static string GenerateKey()
    {
        int x1 = Rnd.Next(1, 8);
        int x2 = Rnd.Next(1, 8);
        int y = x1 / x2;
        int x3 = (int)(Math.Pow(x1, x2) + Math.Pow(x2, y) + Math.Pow(x1 * x2, 2));
        return $" {x1} {x2} {x3}";
    }
}