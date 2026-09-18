namespace AtIPT.Domain;

/// <summary>
/// A single numeric parameter with bounds. Used for both the fixed "model parameters"
/// (FormParamMain tabs) and the adjustable "analysis parameters" (AnalysisForm grid).
/// </summary>
public sealed class Parameter
{
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public double Value { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public bool Enabled { get; set; } = true;

    public Parameter() { }

    public Parameter(string name, string unit = "")
    {
        Name = name;
        Unit = unit;
    }

    public Parameter Clone() => new()
    {
        Name = Name,
        Unit = Unit,
        Value = Value,
        Min = Min,
        Max = Max,
        Enabled = Enabled
    };
}