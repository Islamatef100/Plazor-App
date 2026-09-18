namespace AtIPT.Components.Shared;

public sealed class ChartSeries
{
    public string Name { get; set; } = "";
    public string Color { get; set; } = "#0d6efd";
    public IReadOnlyList<double> X { get; set; } = Array.Empty<double>();
    public IReadOnlyList<double> Y { get; set; } = Array.Empty<double>();
    public bool Visible { get; set; } = true;

    /// <summary>Draw a dot at each data point.</summary>
    public bool PointMarkers { get; set; }

    /// <summary>Skip the connecting line — draw only the markers.</summary>
    public bool PointOnly { get; set; }

    /// <summary>Text used in the tooltip value line, matching old crosshair pattern.</summary>
    public string TooltipLabel { get; set; } = "Value";
}

public sealed class AxisRange
{
    public double XMin { get; set; }
    public double XMax { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
}