using System.Globalization;
using System.Text;
using AtIPT.Application.Abstractions;
using AtIPT.Domain;

namespace AtIPT.Infrastructure.Persistence;

public sealed class ParameterFileService : IParameterFileService
{
    // ------------------------------------------------------------------
    // Read — direct port of FormParamMain.setParm()
    // ------------------------------------------------------------------
    public async Task<Dictionary<int, double>> ReadAsync(Stream stream, CancellationToken ct = default)
    {
        var result = new Dictionary<int, double>();

        using var reader = new StreamReader(stream);
        string? line;

        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            line = line.Trim();
            if (line.Length == 0) continue;

            var lower = line.ToLowerInvariant();

            // Old code: value = temp[temp.Length - 1] after splitting on whitespace.
            var parts = line.Split(new[] { ' ', '\t' },
                                   StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            if (!double.TryParse(parts[^1],
                                 NumberStyles.Any,
                                 CultureInfo.InvariantCulture,
                                 out var val))
                continue;

            int? idx = null;

            // Exact same ordering as the old if/else chain so that
            // e.g. "outer zone permeability" wins over "permeability",
            // and "injection fluid viscosity" wins over "formation fluid viscosity".
            if (lower.Contains("shut in time")) idx = 0;
            else if (lower.Contains("injection rate")) idx = 7;
            else if (lower.Contains("level skip")) idx = 17;
            else if (lower.Contains("formation thickness")
                  || lower.Contains("injection layer thickness")) idx = 9;
            else if (lower.Contains("wellbore volume")) idx = 4;
            else if (lower.Contains("porosity")) idx = 10;
            else if (lower.Contains("fluid compressibility")) idx = 5;
            else if (lower.Contains("injection fluid viscosity")) idx = 6;
            else if (lower.Contains("outer zone permeability")) idx = 26;
            else if (lower.Contains("permeability")) idx = 18;
            else if (lower.Contains("formation volume factor")) idx = 11;
            else if (lower.Contains("poisson's ratio")) idx = 12;
            else if (lower.Contains("young's modulus")) idx = 13;
            else if (lower.Contains("total compressibility")) idx = 14;
            else if (lower.Contains("shut-in pressure")) idx = 1;
            else if (lower.Contains("residual oil saturation")
                  || lower.Contains("pore fluid residual saturation")) idx = 15;
            else if (lower.Contains("injected fluid initial saturation")
                  || lower.Contains("initial water saturation")) idx = 16;
            else if (lower.Contains("volume injected")) idx = 8;
            else if (lower.Contains("mobility ratio")) idx = 19;
            else if (lower.Contains("diffusivity ratio")) idx = 20;
            else if (lower.Contains("closure time")) idx = 22;
            else if (lower.Contains("fracture half length")) idx = 23;
            else if (lower.Contains("mobility front")) idx = 24;
            else if (lower.Contains("t- end") || lower.Contains("t-end")) idx = 2;
            else if (lower.Contains("p-end") || lower.Contains("p- end")) idx = 3;
            else if (lower.Contains("fracture storage constant")) idx = 21;
            else if (lower.Contains("fcd")
                  || lower.Contains("dimensionless fracture conductivity")) idx = 27;
            else if (lower.Contains("fracture skin")) idx = 28;
            else if (lower.Contains("delpat")) idx = 29;
            else if (lower.Contains("injection layer stress")) idx = 30;
            else if (lower.Contains("containment layer stress")) idx = 31;
            else if (lower.Contains("toughness")) idx = 32;
            else if (lower.Contains("formation fluid viscosity")) idx = 25;
            else if (lower.Contains("shrinkage start time")) idx = 33;

            if (idx.HasValue)
                result[idx.Value] = val;
        }

        return result;
    }

    // ------------------------------------------------------------------
    // Write — direct port of FormParamMain.saveToFile_Click()
    // ------------------------------------------------------------------
    public string Write(SimCase c)
    {
        var p = c.Parameters;
        var sb = new StringBuilder();

        void Line(string label, double val)
            => sb.AppendLine(label + "\t" + val.ToString(CultureInfo.InvariantCulture));

        void Raw(string s) => sb.AppendLine(s);

        Line("Shut In Time (hr)", p[0].Value);
        Line("Injection Rate (bpd)", p[7].Value);
        Raw("Plotting Level Skip 0.01");
        Line("Injection Layer Thickness (ft)", p[9].Value);
        Line("Wellbore Volume (cb-ft)", p[4].Value);
        Line("Porosity (Fraction)", p[10].Value);
        Line("Injection Fluid Compressibility (1/psi)", p[5].Value);
        Line("Injection Fluid Viscosity (cP)", p[6].Value);
        Raw("-\t0");
        Line("Inner Zone Permeability  (mD)", p[18].Value);
        Line("Formation Volume Factor (bbl/stb)", p[11].Value);
        Line("Poisson's Ratio", p[12].Value);
        Line("Young's Modulus (psi)", p[13].Value);
        Line("Total Compressibility (1/psi)", p[14].Value);
        Line("Shut-in Pressure (psi)", p[1].Value);
        Line("Residual Oil Saturation (Fraction)", p[15].Value);
        Line("Initial Water Saturation (Fraction) ", p[16].Value);
        Line("Volume Injected (bbl)", p[8].Value);
        Line("Mobility Ratio", p[19].Value);
        Line("Diffusivity Ratio", p[20].Value);
        Line("Fracture Closure Time (hr)", p[22].Value);
        Line("Fracture Half Length (ft)", p[23].Value);
        Line("Mobility Front, Elliptical", p[24].Value);
        Line("T-end (hr)", p[2].Value);
        Line("P-end (psi)", p[3].Value);
        Raw("-\t0");
        Line("Fracture Storage Constant (bbls/psi)", p[21].Value);
        Line("Dimensionless Fracture Conductivity (FCD)", p[27].Value);
        Line("Fracture Skin", p[28].Value);
        Line("Length Shrinkage Speed Parameter (Delpat) ", p[29].Value);
        Line("Injection Layer stress (psi)", p[30].Value);
        Line("Containment Layer Stress (psi)", p[31].Value);
        Line("Fracture Toughness (psi.in^1/2)", p[32].Value);
        Line("Formation Fluid Viscosity (cP)", p[25].Value);
        Line("Outer Zone Permeability(mD)", p[26].Value);
        Line("Shrinkage Start Time (hr)", p[33].Value);

        return sb.ToString();
    }
}