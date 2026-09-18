using System.Globalization;
using AtIPT.Application.Abstractions;
using AtIPT.Domain;

namespace AtIPT.Infrastructure.Import;

public sealed class TextFileImportService : IImportService
{
    public IReadOnlyList<Variable> ReadFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToArray();

        if (lines.Length == 0) return Array.Empty<Variable>();

        var (delims, colNames) = DetectFormat(lines);

        var columns = colNames
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => new Variable(n))
            .ToList();

        if (columns.Count == 0) return columns;

        for (int r = 1; r < lines.Length; r++)
        {
            var cells = lines[r].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length != columns.Count) continue;

            for (int c = 0; c < columns.Count; c++)
            {
                if (double.TryParse(cells[c], NumberStyles.Any,
                                    CultureInfo.InvariantCulture, out var v))
                {
                    columns[c].Values.Add(v);
                }
            }
        }

        return columns;
    }

    public IReadOnlyList<string> GetColumnNames(string filePath)
    {
        var first = File.ReadLines(filePath).FirstOrDefault() ?? "";
        var (_, names) = DetectFormat(new[] { first, "" });
        return names;
    }

    // ---------------------------------------------------------------
    // Format detection — try every delimiter, pick the one where the
    // header column count matches the most data rows.
    // ---------------------------------------------------------------

    private static (char[] delims, string[] names) DetectFormat(string[] lines)
    {
        int testRows = Math.Min(lines.Length - 1, 5);

        var candidates = new List<(char[] delims, string[] names, int matches)>();

        // 1) Tab
        AddCandidate(candidates, lines, new[] { '\t' },
            SplitAndCorrect(lines[0], new[] { '\t' }), testRows);

        // 2) Comma
        AddCandidate(candidates, lines, new[] { ',' },
            SplitAndCorrect(lines[0], new[] { ',' }), testRows);

        // 3) Semicolon
        AddCandidate(candidates, lines, new[] { ';' },
            SplitAndCorrect(lines[0], new[] { ';' }), testRows);

        // 4) Whitespace on the RAW header
        AddCandidate(candidates, lines, new[] { ' ', '\t' },
            SplitAndCorrect(lines[0], new[] { ' ', '\t' }), testRows);

        // 5) Whitespace on the CORRECTED header
        //    (matches the old LasFileExtraction behaviour exactly)
        var corrected = ApplyHeaderCorrection(lines[0]);
        var names5 = corrected.Split(new[] { ' ', '\t' },
                                     StringSplitOptions.RemoveEmptyEntries);
        AddCandidate(candidates, lines, new[] { ' ', '\t' }, names5, testRows);

        var best = candidates
            .OrderByDescending(c => c.matches)
            .ThenByDescending(c => c.names.Length)
            .FirstOrDefault();

        if (best.names == null || best.names.Length == 0)
            return (new[] { ' ', '\t' }, new[] { lines[0] });

        return (best.delims, best.names);
    }

    private static void AddCandidate(
        List<(char[] delims, string[] names, int matches)> list,
        string[] lines,
        char[] delims,
        string[] names,
        int testRows)
    {
        if (names.Length < 1) return;

        int matches = 0;
        for (int r = 1; r <= testRows && r < lines.Length; r++)
        {
            var cells = lines[r].Split(delims, StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length == names.Length) matches++;
        }

        list.Add((delims, names, matches));
    }

    private static string[] SplitAndCorrect(string line, char[] delims)
    {
        return line
            .Split(delims, StringSplitOptions.RemoveEmptyEntries)
            .Select(ApplyHeaderCorrection)
            .ToArray();
    }

    /// <summary>
    /// Direct port of allVars.InputHeaderCorrection.
    /// Removes spaces inside the header, but re-inserts a space after a
    /// closing parenthesis that follows a (group).
    /// </summary>
    public static string ApplyHeaderCorrection(string val)
    {
        int open = 0, close = 0;
        var sb = new System.Text.StringBuilder(val.Length);

        for (int i = 0; i < val.Length; i++)
        {
            if (i + 1 < val.Length && val[i] == '~' && val[i + 1] == 'A')
            {
                i++;
                continue;
            }

            if (open == 1 && close == 1)
            {
                sb.Append(' ');
                open = close = 0;
            }

            if (val[i] == '(') open++;
            if (val[i] == ')') close++;
            if (val[i] == ' ') continue;

            sb.Append(val[i]);
        }

        return sb.ToString().Trim();
    }
}