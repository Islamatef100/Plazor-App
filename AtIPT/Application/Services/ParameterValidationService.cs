using AtIPT.Domain;

namespace AtIPT.Application.Services;

/// <summary>
/// Parameter validation.
///
/// Only Enabled parameters are validated. Disabled parameters are hidden
/// in the UI and must not be checked — that was the root cause of the
/// false "Diffusivity Ratio has to be between Min and Max" error, since
/// Diffusivity Ratio is disabled by default with Val = 0 while its
/// Min = 0.1.
///
/// Error messages include Parameter name, current value, Min and Max,
/// matching the style requested in the issue tracker.
/// </summary>
public static class ParameterValidationService
{
    public static string? ValidateBounds(SimCase c, double shutInPressure)
    {
        var p = c.AnalysisParameters;

        // Min/Max sanity checks on the visible parameters.
        for (int i = 0; i < p.Count; i++)
        {
            if (!p[i].Enabled) continue;

            if (p[i].Min > p[i].Max)
                return $"{p[i].Name}: Min value ({p[i].Min}) must be less than Max value ({p[i].Max}).";
        }

        // Mobility Ratio special rule: min > 0 and (min < 1 → max ≤ 1)
        if (p[10].Enabled)
        {
            if (p[10].Min < 0 || (p[10].Min < 1 && p[10].Max > 1))
                return " Please  enter a value to Mobility Ratio that agrees with these rules \n \n "
                     + "1) Min must be > 0 and  Max >= Min \n \n "
                     + "2) If Min <= 1 then Max must be <= 1";
        }

        // Shrinkage-driven checks
        if (c.ShrinkageType != ShrinkageType.None)
        {
            if (p[6].Min < p[5].Max)
                return "Min Value of Containment Layer Stress must be greater than Max Value of Injection Layer Stress";

            if (!(p[5].Max < shutInPressure && shutInPressure < p[6].Min))
                return "Shut in pressure MUST be between injection layer stress and Containment Layer Stress";
        }

        return null;
    }

    public static string? ValidateUserGuess(SimCase c, double shutInPressure)
    {
        var p = c.AnalysisParameters;

        // Every visible parameter: User Guess must be inside [Min, Max].
        for (int i = 0; i < p.Count; i++)
        {
            if (!p[i].Enabled) continue;

            if (p[i].Value < p[i].Min || p[i].Value > p[i].Max)
            {
                return $"{p[i].Name}: User Guess value {p[i].Value} "
                     + $"is outside the allowed range (Min: {p[i].Min}, Max: {p[i].Max}).";
            }
        }

        // Mobility Ratio special rule
        if (p[10].Enabled)
        {
            if (p[10].Min < 0 || (p[10].Min < 1 && p[10].Max > 1))
                return " Please  enter a value to Mobility Ratio that agrees with these rules \n \n "
                     + "1) Min must be > 0 and  Max >= Min \n \n "
                     + "2) If Min <= 1 then Max must be <= 1";
        }

        // Shrinkage-driven checks
        if (c.ShrinkageType != ShrinkageType.None)
        {
            if (p[6].Min < p[5].Max)
                return "Min Value of Containment Layer Stress must be greater than Max Value of Injection Layer Stress";

            if (!(p[5].Max < shutInPressure && shutInPressure < p[6].Min))
                return "Shut in pressure MUST be between injection layer stress and Containment Layer Stress";
        }

        return null;
    }
}