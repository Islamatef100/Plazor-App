namespace AtIPT.Helpers;

public static class MathHelpers
{
    /// <summary>Inverse hyperbolic sine — used by every "Mobility Front" calculation.</summary>
    public static double ASinh(double value) => Math.Log(value + Math.Sqrt(value * value + 1));

    /// <summary>Natural log kept for parity with the old allVars.ln helper.</summary>
    public static double Ln(double value) => Math.Log(value) / Math.Log(Math.E);

    /// <summary>Fracture storage constant. Direct port of AnalysisForm.getFracStorage.</summary>
    public static double GetFracStorage(
        double fracHalfLength,
        double injLayerThickness,
        double poissonRatio,
        double youngModulus,
        int fractureType)
    {
        double fracStorage;

        if (fractureType == 0)          // KGD
        {
            fracStorage = 2 * Math.PI * (1 - Math.Pow(poissonRatio, 2))
                        / youngModulus * injLayerThickness * Math.Pow(fracHalfLength, 2);
        }
        else if (fractureType == 1)     // PKN
        {
            fracStorage = Math.PI * (1 - Math.Pow(poissonRatio, 2))
                        / youngModulus * fracHalfLength * Math.Pow(injLayerThickness, 2);
        }
        else                            // Ellipsoidal
        {
            double v1 = injLayerThickness / (2 * fracHalfLength);
            double v2 = 2 * fracHalfLength / injLayerThickness;
            double m = 1 - Math.Min(v1, v2);

            double EofM = 1.0
                        - 0.25 * Math.Pow(m, 2)
                        - 0.04687500 * Math.Pow(m, 4)
                        - 0.01953125 * Math.Pow(m, 6)
                        - 0.01068115 * Math.Pow(m, 8)
                        - 0.00672913 * Math.Pow(m, 10);

            fracStorage = 2 * Math.PI / 3
                        * ((1 - Math.Pow(poissonRatio, 2)) / youngModulus)
                        * (Math.Min(2 * fracHalfLength, injLayerThickness) / EofM)
                        * fracHalfLength * injLayerThickness;
        }

        return fracStorage / 5.615;
    }
}