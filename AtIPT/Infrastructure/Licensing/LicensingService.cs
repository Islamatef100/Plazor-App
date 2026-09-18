using AtIPT.Application.Abstractions;

namespace AtIPT.Infrastructure.Licensing;

public sealed class LicensingService : ILicensingService
{
    // Same value as the old Program.cs.
    private static readonly DateTime ExpirationDate = new(2055, 12, 15);

    public string? LastError { get; private set; }

    public bool IsLicensed()
    {
        if (DateTime.UtcNow <= ExpirationDate) return true;
        LastError = "Your @IPT application period has expired.  Please contact Advantek International to re-active your license";
        return false;
    }
}