using System.Text.RegularExpressions;

namespace AtIPT.Infrastructure.Simulation;

/// <summary>
/// Reads best_history.txt (one CSV row per solution) and Configuration.txt
/// (model type / shrinkage type / fracture type, one per line).
/// Port of AtIPT.BestHistoryParser from the old project.
/// </summary>
public static class BestHistoryParser
{
    public static List<List<string>> ReadBestHistory(string exeFolder)
    {
        var rows = new List<List<string>>();
        var path = Path.Combine(exeFolder, "best_history.txt");

        if (!File.Exists(path)) return rows;

        foreach (var raw in File.ReadAllLines(path).Skip(1))
        {
            var line = Regex.Replace(raw.Trim(), @"\s+", " ");
            if (line.Length == 0) continue;

            var cells = line.Split(',').Select(s => s.Trim()).ToList();
            rows.Add(cells);
        }

        return rows;
    }

    public static List<string> ReadBestHistoryConfiguration(string exeFolder)
    {
        var list = new List<string>();
        var path = Path.Combine(exeFolder, "Configuration.txt");

        if (!File.Exists(path)) return list;

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            list.Add(line);
        }

        return list;
    }
}