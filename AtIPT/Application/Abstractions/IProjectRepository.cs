using AtIPT.Domain;

namespace AtIPT.Application.Abstractions;

public interface IProjectRepository
{
    Task<string> CreateProjectAsync(string projectName, CancellationToken ct = default);
    Task SaveCaseAsync(string folderPath, SimCase simCase, string? inputFilePath, CancellationToken ct = default);
    Task<SimCase?> LoadCaseAsync(string folderPath, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectNode>> ListProjectsAsync();
    Task DeleteProjectAsync(string projectName);
    Task RenameProjectAsync(string oldName, string newName);
}