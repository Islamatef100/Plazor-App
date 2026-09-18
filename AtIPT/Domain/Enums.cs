namespace AtIPT.Domain;

/// <summary>
/// Model selection values. Order must match the old ComboBox1 index in AnalysisForm
/// because these values are written directly into Solver_Input.dat / text files.
/// </summary>
public enum ModelSelection
{
    InfiniteConductivity = 0,
    FiniteSingleMobility = 1,
    FiniteDualMobility = 2
}

/// <summary>
/// Shrinkage type values. Order matches the old ComboBox2 index in AnalysisForm.
/// Note: values 1, 2, 3 all behave as "1" in the solver; the enum keeps them for
/// UI compatibility with the original application.
/// </summary>
public enum ShrinkageType
{
    None = 0,
    HeightOnly = 1,
    LengthOnly = 2,
    Both = 3
}

/// <summary>
/// Fracture type values. Order matches the old ComboBox3 index in AnalysisForm.
/// In solver input files, the value written is (int)FractureType + 1.
/// </summary>
public enum FractureType
{
    KGD = 0,
    PKN = 1,
    Ellipsoidal = 2
}