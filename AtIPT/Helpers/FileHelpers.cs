using System.Globalization;

namespace AtIPT.Helpers;

public static class FileHelpers
{
    public static string ExeFolder(string contentRoot) => Path.Combine(contentRoot, "Exe");

    public static string ProjectsRoot(string contentRoot) => Path.Combine(contentRoot, "Projects");

    public static string SanitiseFolderName(string name)
    {
        foreach (var ch in Path.GetInvalidFileNameChars())
            name = name.Replace(ch, '_');
        return name.Trim();
    }

    /// <summary>
    /// A solver output file is considered valid if it contains at least
    /// one line with three numeric values. This matches the old project's
    /// add_data() behaviour more closely than a header-only check.
    /// </summary>
    public static async Task<bool> IsSolverOutputValidAsync(string path)
    {
        if (!File.Exists(path)) return false;

        var lines = await File.ReadAllLinesAsync(path);
        if (lines.Length == 0) return false;

        // If the file literally contains only the word "bad arguments"
        // on its first line and nothing else, treat it as invalid.
        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        if (nonEmpty.Length == 0) return false;
        if (nonEmpty.Length == 1
            && nonEmpty[0].Trim().Equals("bad arguments", StringComparison.OrdinalIgnoreCase))
            return false;

        // Otherwise require at least one line with three numbers.
        foreach (var raw in nonEmpty)
        {
            var parts = raw.Trim().Split(new[] { ' ', '\t', ',' },
                                         StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) continue;

            if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _) &&
                double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                return true;
            }
        }

        return false;
    }
}