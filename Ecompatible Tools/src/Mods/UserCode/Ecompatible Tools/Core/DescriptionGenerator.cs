using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Linq;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        private const int Position = 16;
        public LocString BaseValue(float value) => TextLoc.InfoLoc($"Base value: {Text.MonoPos(Position, value.ToString("0.00"))}");
        public LocString BaseValue(int value) => TextLoc.InfoLoc($"Base value: {Text.MonoPos(Position, value.ToString("0"))}");
        public LocString Multiplier(string multiplierName, float multiplier) => TextLoc.InfoLoc($"{multiplierName}: {Text.StyledMonoPosPercent(multiplier - 1, Position, true)}");
        public LocString BuildModificationListDescriptionInt(AuxillaryInfo auxillaryInfo)
        {
            StepOutput baseOutput = auxillaryInfo.StepOutputs.LastOrDefault(step => step.ModificationType == ModificationType.BaseValue);
            if (baseOutput == null) return LocString.Empty;
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            for (int i = baseOutput.Step; i < auxillaryInfo.StepOutputs.Length; i++)
            {
                locStringBuilder.AppendLineIfSet(auxillaryInfo.StepOutputs[i].Description);
            }
            locStringBuilder.AppendLine(Localizer.NotLocalizedStr("-----------------------------"));
            locStringBuilder.AppendLine(TextLoc.BoldLoc($"Result: {Text.MonoPos(Position, auxillaryInfo.StepOutputs[auxillaryInfo.StepOutputs.Length - 1].IntOutput, format:"0")}"));
            return locStringBuilder.ToLocString();
        }
    }
}
