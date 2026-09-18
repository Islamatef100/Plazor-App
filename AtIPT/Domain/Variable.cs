namespace AtIPT.Domain;

/// <summary>
/// A named series of numeric values (a column of raw data, a computed curve, etc.).
/// Values has a public setter so the project file (JSON) can round-trip.
/// </summary>
public sealed class Variable
{
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public List<double> Values { get; set; } = new();

    public Variable() { }

    public Variable(string name)
    {
        Name = name;
    }

    public Variable(string name, string unit)
    {
        Name = name;
        Unit = unit;
    }

    public Variable Clone()
    {
        var copy = new Variable(Name, Unit);
        copy.Values.AddRange(Values);
        return copy;
    }
}