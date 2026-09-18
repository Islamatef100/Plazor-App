using System.Text.Json;
using AtIPT.Application.Abstractions;
using AtIPT.Domain;
using AtIPT.Helpers;

namespace AtIPT.Infrastructure.Persistence;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly string _root;

    public ProjectRepository(string contentRoot)
    {
        _root = FileHelpers.ProjectsRoot(contentRoot);
        Directory.CreateDirectory(_root);
    }

    public Task<string> CreateProjectAsync(string projectName, CancellationToken ct = default)
    {
        var safeName = FileHelpers.SanitiseFolderName(projectName);
        var folder = Path.Combine(_root, safeName);
        Directory.CreateDirectory(folder);

        var caseFolder = Path.Combine(folder, "0");
        Directory.CreateDirectory(caseFolder);

        return Task.FromResult(caseFolder);
    }

    public async Task SaveCaseAsync(string folderPath, SimCase simCase, string? inputFilePath, CancellationToken ct = default)
    {
        Directory.CreateDirectory(folderPath);

        var json = JsonSerializer.Serialize(simCase, JsonOptions);
        await File.WriteAllTextAsync(Path.Combine(folderPath, "Case.xml"), json, ct);

        if (!string.IsNullOrEmpty(inputFilePath) && File.Exists(inputFilePath))
        {
            var dest = Path.Combine(folderPath, "Raw_Data_Input.txt");
            try { File.Copy(inputFilePath, dest, overwrite: true); } catch { }
        }
    }

    public async Task<SimCase?> LoadCaseAsync(string folderPath, CancellationToken ct = default)
    {
        var file = Path.Combine(folderPath, "Case.xml");
        if (!File.Exists(file)) return null;

        var json = await File.ReadAllTextAsync(file, ct);
        return JsonSerializer.Deserialize<SimCase>(json, JsonOptions);
    }

    public Task<IReadOnlyList<ProjectNode>> ListProjectsAsync()
    {
        var list = new List<ProjectNode>();

        foreach (var dir in Directory.GetDirectories(_root))
        {
            var node = new ProjectNode(Path.GetFileName(dir), dir);

            foreach (var caseDir in Directory.GetDirectories(dir).OrderBy(x => x))
            {
                var folderName = Path.GetFileName(caseDir);
                var caseNode = new ProjectNode($"Case #{folderName}", folderName);
                caseNode.AddChild(new ProjectNode("Inputs", "Inputs"));
                caseNode.AddChild(new ProjectNode("Solutions", "Solutions"));
                node.AddChild(caseNode);
            }

            list.Add(node);
        }

        return Task.FromResult<IReadOnlyList<ProjectNode>>(list);
    }

    public Task DeleteProjectAsync(string projectName)
    {
        var folder = Path.Combine(_root, FileHelpers.SanitiseFolderName(projectName));
        if (Directory.Exists(folder)) Directory.Delete(folder, true);
        return Task.CompletedTask;
    }

    public Task RenameProjectAsync(string oldName, string newName)
    {
        var src = Path.Combine(_root, FileHelpers.SanitiseFolderName(oldName));
        var dst = Path.Combine(_root, FileHelpers.SanitiseFolderName(newName));
        if (Directory.Exists(src) && !Directory.Exists(dst))
            Directory.Move(src, dst);
        return Task.CompletedTask;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true
    };
}