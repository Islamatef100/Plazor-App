namespace AtIPT.Domain;

public sealed class SimCase
{
    public List<Parameter> Parameters { get; set; } = new();
    public List<Parameter> AnalysisParameters { get; set; } = new();

    public List<Variable> ImportedVariables { get; set; } = new();
    public List<Variable> RawVariables { get; set; } = new();
    public List<Variable> OriginalRawData { get; set; } = new();
    public List<Variable> DifferenceVariables { get; set; } = new();

    public List<Variable> InitialGuessMin { get; set; } = new();
    public List<Variable> InitialGuessMax { get; set; } = new();
    public List<Variable> SimulationResults { get; set; } = new();
    public List<Variable[]> Runs { get; set; } = new();
    public List<Parameter[]> RunParameters { get; set; } = new();
    public List<double> SolutionFitnesses { get; set; } = new();

    public string[] SelectedInputs { get; set; } = new string[3];

    public ModelSelection ModelSelection { get; set; } = ModelSelection.InfiniteConductivity;
    public ShrinkageType ShrinkageType { get; set; } = ShrinkageType.None;
    public FractureType FractureType { get; set; } = FractureType.KGD;
    public double? TestGuessFitness { get; set; }
    public int ShutInPoint { get; set; }
    public int NumRuns { get; set; }
    public int CurrentRunIndex { get; set; }
    public int RawVariablesCount { get; set; }

    public void ParametersInit()
    {
        Parameters.Clear();

        Parameters.Add(new Parameter("Shut-in Time (hr)", "hr"));
        Parameters[0].Value = 0;

        Parameters.Add(new Parameter("Shut-in Pressure (psi)", "psi"));
        Parameters[1].Value = 0;

        Parameters.Add(new Parameter("End Point Time (hr)", "hr"));
        Parameters[2].Value = 0;

        Parameters.Add(new Parameter("End Point Pressure (psi)", "psi"));
        Parameters[3].Value = 0;

        Parameters.Add(new Parameter("Wellbore Volume (cb-ft)", "cb-ft"));
        Parameters[4].Value = 0;
        Parameters[4].Enabled = false;

        Parameters.Add(new Parameter("Injection Fluid Compressibility (1/psi)", "1/psi"));
        Parameters[5].Value = 0;

        Parameters.Add(new Parameter("Injection Fluid Viscosity (cP)", "cP"));
        Parameters[6].Value = 0;

        Parameters.Add(new Parameter("Injection Rate (bpd)", "bpd"));
        Parameters[7].Value = 0;

        Parameters.Add(new Parameter("Volume Injected (bbl)", "bbl"));
        Parameters[8].Value = 0;

        Parameters.Add(new Parameter("Injection Layer Thickness (ft)", "ft"));
        Parameters[9].Value = 0;

        Parameters.Add(new Parameter("Porosity (Fraction)"));
        Parameters[10].Value = 0;

        Parameters.Add(new Parameter("Formation Volume Factor (bbl/stb)"));
        Parameters[11].Value = 0;

        Parameters.Add(new Parameter("Poisson's Ratio"));
        Parameters[12].Value = 0;

        Parameters.Add(new Parameter("Young's Modulus (psi)", "psi"));
        Parameters[13].Value = 0;

        Parameters.Add(new Parameter("Total Compressibility (1/psi)", "1/psi"));
        Parameters[14].Value = 0;

        Parameters.Add(new Parameter("Residual Oil Saturation (Fraction)"));
        Parameters[15].Value = 0;

        Parameters.Add(new Parameter("Initial Water Saturation (Fraction)"));
        Parameters[16].Value = 0;

        Parameters.Add(new Parameter("Plotting Level Skip"));
        Parameters[17].Value = 0.01;

        Parameters.Add(new Parameter("Inner Zone Permeability (mD)", "mD"));
        Parameters[18].Value = 0;

        Parameters.Add(new Parameter("Mobility Ratio"));
        Parameters[19].Value = 0;

        Parameters.Add(new Parameter("Diffusivity Ratio"));
        Parameters[20].Value = 0;

        Parameters.Add(new Parameter("Fracture Storage Constant (bbls/psi)", "bbls/psi"));
        Parameters[21].Value = 0;

        Parameters.Add(new Parameter("Fracture Closure Time (hr)", "hr"));
        Parameters[22].Value = 0;

        Parameters.Add(new Parameter("Fracture Half Length (ft)", "ft"));
        Parameters[23].Value = 0;

        Parameters.Add(new Parameter("Mobility Front, Eliptical"));
        Parameters[24].Value = 0;

        Parameters.Add(new Parameter("Formation Fluid Viscosity (cP)", "cP"));
        Parameters[25].Value = 1;

        Parameters.Add(new Parameter("Outer Zone Permeability (mD)", "mD"));
        Parameters[26].Value = 0;

        Parameters.Add(new Parameter("Dimensionless Fracture Conductivity (FCD) ", "(FCD)"));
        Parameters[27].Value = 0;

        Parameters.Add(new Parameter("Fracture Skin"));
        Parameters[28].Value = 0;

        Parameters.Add(new Parameter("Length Shrinkage Speed Parameter (Delpat)", "(Delpat)"));
        Parameters[29].Value = 0;

        Parameters.Add(new Parameter("Injection Layer Stress (psi)", "(psi)"));
        Parameters[30].Value = 0;

        Parameters.Add(new Parameter("Containment Layer Stress (psi)", "(psi)"));
        Parameters[31].Value = 0;

        Parameters.Add(new Parameter("Fracture Toughness (psi.in^1/2)", "(psi.in^1/2)"));
        Parameters[32].Value = 0;

        Parameters.Add(new Parameter("Shrinkage Start Time (hr)", "(hr)"));
        Parameters[33].Value = 0;
    }

    public void AnalysisParametersInit()
    {
        AnalysisParameters.Clear();

        AnalysisParameters.Add(new Parameter("Fracture Half Length (ft)"));
        AnalysisParameters[0].Min = 0;
        AnalysisParameters[0].Max = 0;

        AnalysisParameters.Add(new Parameter("Fracture Storage Constant (bbls/psi)"));
        AnalysisParameters[1].Min = 0;
        AnalysisParameters[1].Max = 0;

        AnalysisParameters.Add(new Parameter("DimensionLess Fracture Storage Constant "));
        AnalysisParameters[2].Enabled = true;
        AnalysisParameters[2].Min = 0;
        AnalysisParameters[2].Max = 0;

        AnalysisParameters.Add(new Parameter("DimensionLess Wellbore Storage Constant"));
        AnalysisParameters[3].Min = 0;
        AnalysisParameters[3].Max = 0;

        AnalysisParameters.Add(new Parameter("Inner Zone Permeability (mD)"));
        AnalysisParameters[4].Min = 0;
        AnalysisParameters[4].Max = 0;

        AnalysisParameters.Add(new Parameter("Injection Layer Stress (psi)"));
        AnalysisParameters[5].Min = 0;
        AnalysisParameters[5].Max = 0;

        AnalysisParameters.Add(new Parameter("Containment Layer Stress (psi)"));
        AnalysisParameters[6].Min = 0;
        AnalysisParameters[6].Max = 0;

        AnalysisParameters.Add(new Parameter("Length Shrinkage Speed Parameter (Delpat)"));
        AnalysisParameters[7].Min = 0;
        AnalysisParameters[7].Max = 0;

        AnalysisParameters.Add(new Parameter("Fracture Skin"));
        AnalysisParameters[8].Min = 0;
        AnalysisParameters[8].Max = 1;
        AnalysisParameters[8].Enabled = false;

        AnalysisParameters.Add(new Parameter("Dimensionless Fracture Conductivity (FCD)"));
        AnalysisParameters[9].Min = 0.1;
        AnalysisParameters[9].Max = 10;
        AnalysisParameters[9].Enabled = false;

        AnalysisParameters.Add(new Parameter("Mobility Ratio"));
        AnalysisParameters[10].Value = 0.15;
        AnalysisParameters[10].Min = 0.1;
        AnalysisParameters[10].Max = 0.9;
        AnalysisParameters[10].Enabled = true;

        AnalysisParameters.Add(new Parameter("Diffusivity Ratio"));
        AnalysisParameters[11].Enabled = false;
        AnalysisParameters[11].Value = 0.15;
        AnalysisParameters[11].Min = 0.1;
        AnalysisParameters[11].Max = 0.9;

        AnalysisParameters.Add(new Parameter("Mobility Front"));
        AnalysisParameters[12].Value = 0.5;
        AnalysisParameters[12].Min = 0.1;
        AnalysisParameters[12].Max = 5;

        AnalysisParameters.Add(new Parameter("RMS Error"));
        AnalysisParameters[13].Enabled = false;
    }

    public int GetIndex(List<Variable> list, string name)
    {
        for (int i = 0; i < list.Count; i++)
            if (list[i].Name == name) return i;
        return -1;
    }

    public void AnalysisParametersRand()
    {
        var r = new Random();
        for (int i = 0; i < 10; i++)
        {
            AnalysisParameters[i].Value =
                (AnalysisParameters[i].Max - AnalysisParameters[i].Min) * r.NextDouble()
                + AnalysisParameters[i].Min;
        }
    }

    public void DataDifferNew()
    {
        ShutInPoint = 0;

        DifferenceVariables.Clear();
        DifferenceVariables.Add(new Variable("Elapsed Time (hr)"));
        DifferenceVariables.Add(new Variable("DeltaP, Field Data"));
        DifferenceVariables.Add(new Variable("Derivative, Field Data"));
        DifferenceVariables.Add(new Variable("Pressure Decline"));
        DifferenceVariables.Add(new Variable("time sqr"));

        if (RawVariables.Count < 2 || RawVariables[1].Values.Count == 0)
            return;

        var deltaTimeList = new List<double>();
        var deltaPressureList = new List<double>();

        double shutInTime = Parameters[0].Value;
        double shutInPressure = Parameters[1].Value;

        while (ShutInPoint < RawVariables[1].Values.Count
               && RawVariables[1].Values[ShutInPoint] < shutInTime)
        {
            ShutInPoint++;
            deltaTimeList.Add(0);
            deltaPressureList.Add(0);
        }

        deltaTimeList.Add(0);
        deltaPressureList.Add(0);

        var time = RawVariables[1].Values;
        var pressure = RawVariables[0].Values;

        for (int i = ShutInPoint + 1; i < time.Count; i++)
        {
            deltaTimeList.Add(time[i] - shutInTime);
            deltaPressureList.Add(shutInPressure - pressure[i]);
        }

        for (int i = ShutInPoint + 1; i < time.Count - 1; i++)
        {
            double deltaTime = deltaTimeList[i];

            double deltaTimeRight = 0;
            double deltaPressureRight = 0;
            double deltaTimeLeft = 0;
            double deltaPressureLeft = 0;

            int j = i + 1;
            while (j < time.Count)
            {
                if (deltaTimeList[j] >= 1.259 * deltaTime)
                {
                    deltaTimeRight = deltaTimeList[j];
                    deltaPressureRight = deltaPressureList[j];
                    break;
                }
                j++;
                if (j == time.Count)
                {
                    deltaTimeRight = time[j - 1] - shutInTime;
                    deltaPressureRight = shutInPressure - pressure[j - 1];
                }
            }

            int k = i - 1;
            while (k >= ShutInPoint)
            {
                if (deltaTimeList[k] <= 0.794 * deltaTime)
                {
                    deltaTimeLeft = deltaTimeList[k];
                    deltaPressureLeft = deltaPressureList[k];
                    break;
                }
                k--;
                if (k == ShutInPoint)
                {
                    deltaTimeLeft = time[k + 1] - shutInTime;
                    deltaPressureLeft = shutInPressure - pressure[k + 1];
                }
            }

            double derivative =
                (deltaPressureRight - deltaPressureLeft)
                / (Math.Log(deltaTimeRight) - Math.Log(deltaTimeLeft));

            DifferenceVariables[0].Values.Add(deltaTimeList[i]);
            DifferenceVariables[1].Values.Add(deltaPressureList[i]);
            DifferenceVariables[2].Values.Add(derivative);
        }
    }

    public void DataDiffer()
    {
        ShutInPoint = 0;

        DifferenceVariables.Clear();
        DifferenceVariables.Add(new Variable("Elapsed Time (hr)"));
        DifferenceVariables.Add(new Variable("DeltaP, Field Data"));
        DifferenceVariables.Add(new Variable("Derivative, Field Data"));
        DifferenceVariables.Add(new Variable("Pressure Decline"));
        DifferenceVariables.Add(new Variable("time sqr"));

        if (RawVariables.Count < 2 || RawVariables[1].Values.Count == 0)
            return;

        while (ShutInPoint < RawVariables[1].Values.Count
               && RawVariables[1].Values[ShutInPoint] < Parameters[0].Value)
            ShutInPoint++;

        double currVal = Math.Log(RawVariables[1].Values[ShutInPoint + 1] - RawVariables[1].Values[ShutInPoint]) / Math.Log(10);
        double sumT = RawVariables[1].Values[ShutInPoint + 1] - RawVariables[1].Values[ShutInPoint];
        double sumP = (RawVariables[0].Values[ShutInPoint] - RawVariables[0].Values[ShutInPoint + 1]) / Parameters[7].Value;
        double sumPD = RawVariables[0].Values[ShutInPoint + 1];
        int ii = 1;
        int iLogData = -1;

        for (int i = ShutInPoint + 1; i < RawVariables[0].Values.Count - 1; i++)
        {
            double dif = Math.Abs(currVal - Math.Log(RawVariables[1].Values[i] - RawVariables[1].Values[ShutInPoint]) / Math.Log(10));

            if (dif >= Parameters[17].Value)
            {
                iLogData++;
                DifferenceVariables[0].Values.Add(sumT / ii);
                DifferenceVariables[1].Values.Add(sumP / ii);

                if (iLogData > 0)
                {
                    DifferenceVariables[2].Values.Add(
                        (DifferenceVariables[1].Values[iLogData] - DifferenceVariables[1].Values[iLogData - 1])
                        / (DifferenceVariables[0].Values[iLogData] - DifferenceVariables[0].Values[iLogData - 1])
                        * 0.5 * (DifferenceVariables[0].Values[iLogData] + DifferenceVariables[0].Values[iLogData - 1]));
                }
                else
                {
                    DifferenceVariables[2].Values.Add(0);
                }

                DifferenceVariables[3].Values.Add(sumPD / ii);
                DifferenceVariables[4].Values.Add(Math.Pow(DifferenceVariables[0].Values[iLogData], 0.5));

                currVal = Math.Log(RawVariables[1].Values[i] - RawVariables[1].Values[ShutInPoint]) / Math.Log(10);
                sumT = RawVariables[1].Values[i] - RawVariables[1].Values[ShutInPoint];
                sumP = (RawVariables[0].Values[ShutInPoint] - RawVariables[0].Values[i]) / Parameters[7].Value;
                sumPD = RawVariables[0].Values[i];
                ii = 1;
            }
            else
            {
                ii++;
                sumT += RawVariables[1].Values[i] - RawVariables[1].Values[ShutInPoint];
                sumP += (RawVariables[0].Values[ShutInPoint] - RawVariables[0].Values[i]) / Parameters[7].Value;
                sumPD += RawVariables[0].Values[i];
            }
        }
    }

    public void CopyFrom(SimCase other)
    {
        Parameters.Clear(); Parameters.AddRange(other.Parameters);
        AnalysisParameters.Clear(); AnalysisParameters.AddRange(other.AnalysisParameters);
        ImportedVariables.Clear(); ImportedVariables.AddRange(other.ImportedVariables);
        RawVariables.Clear(); RawVariables.AddRange(other.RawVariables);
        OriginalRawData.Clear(); OriginalRawData.AddRange(other.OriginalRawData);
        DifferenceVariables.Clear(); DifferenceVariables.AddRange(other.DifferenceVariables);
        InitialGuessMin.Clear(); InitialGuessMin.AddRange(other.InitialGuessMin);
        InitialGuessMax.Clear(); InitialGuessMax.AddRange(other.InitialGuessMax);
        SimulationResults.Clear(); SimulationResults.AddRange(other.SimulationResults);
        Runs.Clear(); Runs.AddRange(other.Runs);
        RunParameters.Clear(); RunParameters.AddRange(other.RunParameters);
        SolutionFitnesses.Clear(); SolutionFitnesses.AddRange(other.SolutionFitnesses);

        SelectedInputs = other.SelectedInputs;
        ModelSelection = other.ModelSelection;
        ShrinkageType = other.ShrinkageType;
        FractureType = other.FractureType;
        ShutInPoint = other.ShutInPoint;
        NumRuns = other.NumRuns;
        CurrentRunIndex = other.CurrentRunIndex;
        RawVariablesCount = other.RawVariablesCount;
        TestGuessFitness = other.TestGuessFitness;
    }
}