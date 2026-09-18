namespace AtIPT.Application.Abstractions;

public interface ILicensingService
{
    /// <summary>Returns true if the app is allowed to start.</summary>
    bool IsLicensed();
    string? LastError { get; }
}