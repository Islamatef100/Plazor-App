using System.Globalization;
using System.Text.RegularExpressions;
using AtIPT.Domain;

namespace AtIPT.Infrastructure.Simulation;

public sealed record SolverOutputResult(
    List<Variable> Series,
    double? Fitness,
    List<string> MatchParameters);

public static class SolverOutputParser
{
    public static List<Variable> Parse(string path) => ParseFull(path).Series;

    public static SolverOutputResult ParseFull(string path)
    {
        var time = new Variable("Elapsed Time (hr)");
        var dp = new Variable("DeltaP, Field Data");
        var der = new Variable("Derivative, Field Data");

        double? fitness = null;
        var matchParams = new List<string>();

        if (!File.Exists(path))
            return new SolverOutputResult(new List<Variable> { time, dp, der }, null, matchParams);

        var lines = File.ReadAllLines(path);
        bool seenHeader = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            if (!seenHeader) { seenHeader = true; continue; }

            line = Regex.Replace(line, @"\s+", " ");
            var sp = line.Split(' ');

            if (sp[0].Equals("fitness", StringComparison.OrdinalIgnoreCase))
            {
                if (sp.Length >= 2 &&
                    double.TryParse(sp[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var f))
                {
                    fitness = f;
                    if (fitness > 1000) fitness = 1000;
                }
                continue;
            }

            if (sp[0].Equals("Match", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < lines.Length)
                {
                    var next = Regex.Replace(lines[i + 1].Trim(), @",", " ");
                    next = Regex.Replace(next, @"\s+", " ").Trim();
                    matchParams = next.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
                break;
            }

            if (sp.Length < 3) continue;
            if (!double.TryParse(sp[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var t)) continue;
            if (!double.TryParse(sp[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) continue;
            if (!double.TryParse(sp[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) continue;

            time.Values.Add(t);
            dp.Values.Add(p);
            der.Values.Add(d);
        }

        return new SolverOutputResult(new List<Variable> { time, dp, der }, fitness, matchParams);
    }
}