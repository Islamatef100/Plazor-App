using AtIPT.Application.Abstractions;
using AtIPT.Domain;
using AtIPT.Helpers;
using System.Globalization;

namespace AtIPT.Application.Services;

public static class SolverInputBuilder
{
    private static string N(double v) => v.ToString("G10", CultureInfo.InvariantCulture);

    public static string Build(SimCase c, RunMode mode)
    {
        double fhl = mode == RunMode.User ? c.AnalysisParameters[0].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[0].Min
                                                 : c.AnalysisParameters[0].Max;
        double perm = mode == RunMode.User ? c.AnalysisParameters[4].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[4].Min
                                                 : c.AnalysisParameters[4].Max;
        double skin = mode == RunMode.User ? c.AnalysisParameters[8].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[8].Min
                                                 : c.AnalysisParameters[8].Max;
        double fcd = mode == RunMode.User ? c.AnalysisParameters[9].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[9].Min
                                                 : c.AnalysisParameters[9].Max;
        double mobRatio = mode == RunMode.User ? c.AnalysisParameters[10].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[10].Min
                                                 : c.AnalysisParameters[10].Max;
        double difRatio = mode == RunMode.User ? c.AnalysisParameters[11].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[11].Min
                                                 : c.AnalysisParameters[11].Max;
        double dimWellSto = mode == RunMode.User ? c.AnalysisParameters[3].Value
                          : mode == RunMode.Min ? c.AnalysisParameters[3].Min
                                                 : c.AnalysisParameters[3].Max;

        double fluConPos = Math.Round(
            MathHelpers.ASinh(
                c.Parameters[8].Value * 2
                / (Math.PI * fhl * fhl * c.Parameters[9].Value * c.Parameters[10].Value
                   * (1 - c.Parameters[15].Value - c.Parameters[16].Value) * 0.1781076))
            / 2, 4);

        double calcDimCloT = Math.Round(
            0.0002637 * perm * c.Parameters[22].Value
            / (c.Parameters[6].Value * c.Parameters[14].Value * c.Parameters[10].Value * fhl * fhl), 4);

        // Update model-side values so downstream code sees the same state
        // the old project produced after its UserGuess_x_y method ran.
        if (mode == RunMode.User) c.AnalysisParameters[12].Value = fluConPos;
        if (mode == RunMode.Min) c.AnalysisParameters[12].Min = fluConPos;
        if (mode == RunMode.Max) c.AnalysisParameters[12].Max = fluConPos;

        double localFracStorage = MathHelpers.GetFracStorage(
            fhl, c.Parameters[9].Value, c.Parameters[12].Value, c.Parameters[13].Value, (int)c.FractureType);

        double dimFracStorage = 5.615 * localFracStorage
                              / (2 * Math.PI * fhl * fhl
                                 * c.Parameters[9].Value * c.Parameters[10].Value * c.Parameters[14].Value);

        // Port of the old project's side-effect:
        // simCase.analysisParameters[2].val = 5.615 * fracStorage / (...);
        if (mode == RunMode.User) c.AnalysisParameters[2].Value = dimFracStorage;
        if (mode == RunMode.Min) c.AnalysisParameters[2].Min = dimFracStorage;
        if (mode == RunMode.Max) c.AnalysisParameters[2].Max = dimFracStorage;

        // t_first — always 0 (old project wrote allVars.dataList[25], which was 0).
        const string tFirst = "0";

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Permeability,Rate,Viscosity,porosity,compressibility, Toughness");
        sb.Append(N(perm)).Append(',')
          .Append(N(c.Parameters[7].Value)).Append(',')
          .Append(N(c.Parameters[6].Value)).Append(',')
          .Append(N(c.Parameters[10].Value)).Append(',')
          .Append(N(c.Parameters[14].Value)).Append(',')
          .Append(N(c.Parameters[32].Value))
          .AppendLine();

        if (c.ShrinkageType != ShrinkageType.None)
        {
            sb.AppendLine("Thickness, sigma1, sigma2, Fracture Half Length, delpat");
            sb.Append(N(c.Parameters[9].Value)).Append(',')
              .Append(N(c.AnalysisParameters[5].Value)).Append(',')
              .Append(N(c.AnalysisParameters[6].Value)).Append(',')
              .Append(N(fhl)).Append(',')
              .Append(N(c.AnalysisParameters[7].Value))
              .AppendLine();
        }
        else
        {
            sb.AppendLine("Thickness, Fracture Half Length");
            sb.Append(N(c.Parameters[9].Value)).Append(',')
              .Append(N(fhl))
              .AppendLine();
        }

        if (c.ShrinkageType != ShrinkageType.None)
        {
            sb.AppendLine("t_shutin,p_shutin,t_end,p_end,ShrinkageStartTime,FracClosureTime");
            sb.Append(N(c.Parameters[0].Value)).Append(',')
              .Append(N(c.Parameters[1].Value)).Append(',')
              .Append(N(c.Parameters[2].Value)).Append(',')
              .Append(N(c.Parameters[3].Value)).Append(',')
              .Append(N(c.Parameters[33].Value)).Append(',')
              .Append(N(c.Parameters[22].Value))
              .AppendLine();

            sb.AppendLine("Shrinkage Type, Fracture Type");
            sb.Append(((int)c.ShrinkageType).ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(((int)c.FractureType + 1).ToString(CultureInfo.InvariantCulture))
              .AppendLine();

            if (c.ModelSelection == ModelSelection.FiniteSingleMobility)
            {
                sb.AppendLine("CfD\tCwD\tXw\tFcD\tSkin");
                sb.Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fcd)).Append(',')
                  .Append(N(skin))
                  .AppendLine();
            }
            else if (c.ModelSelection == ModelSelection.FiniteDualMobility)
            {
                sb.AppendLine("Zita\tCfD\tCwD\tXw\tFcD\tSkin");
                sb.Append(N(difRatio)).Append(',')
                  .Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fcd)).Append(',')
                  .Append(N(skin)).Append(',')
                  .Append(N(fluConPos)).Append(',')
                  .Append(N(mobRatio)).Append(',')
                  .AppendLine();
            }
            else
            {
                sb.AppendLine("zeta\tCfD\tCwD\tXw\tX0\tak");
                sb.Append(N(difRatio)).Append(',')
                  .Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fluConPos)).Append(',')
                  .Append(N(mobRatio))
                  .AppendLine();
            }
        }
        else
        {
            sb.AppendLine("t_shutin,p_shutin,t_end,p_end,t_first");
            sb.Append(N(c.Parameters[0].Value)).Append(',')
              .Append(N(c.Parameters[1].Value)).Append(',')
              .Append(N(c.Parameters[2].Value)).Append(',')
              .Append(N(c.Parameters[3].Value)).Append(',')
              .Append(tFirst)
              .AppendLine();

            if (c.ModelSelection == ModelSelection.FiniteSingleMobility)
            {
                sb.AppendLine("tcl\tzetaCfD\tCwD\tXw\tX0\tak");
                sb.Append(N(calcDimCloT)).Append(',')
                  .Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fcd)).Append(',')
                  .Append(N(skin))
                  .AppendLine();
            }
            else if (c.ModelSelection == ModelSelection.FiniteDualMobility)
            {
                sb.AppendLine("tcl\tzetaCfD\tCwD\tXw\tX0\tak");
                sb.Append(N(calcDimCloT)).Append(',')
                  .Append(N(difRatio)).Append(',')
                  .Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fcd)).Append(',')
                  .Append(N(skin)).Append(',')
                  .Append(N(fluConPos)).Append(',')
                  .Append(N(mobRatio)).Append(',')
                  .AppendLine();
            }
            else
            {
                sb.AppendLine("tcl\tzetaCfD\tCwD\tXw\tX0\tak");
                sb.Append(N(calcDimCloT)).Append(',')
                  .Append(N(difRatio)).Append(',')
                  .Append(N(dimFracStorage)).Append(',')
                  .Append(N(dimWellSto)).Append(",0,")
                  .Append(N(fluConPos)).Append(',')
                  .Append(N(mobRatio))
                  .AppendLine();
            }
        }

        return sb.ToString();
    }
}