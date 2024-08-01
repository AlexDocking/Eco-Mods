using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        public LocString BaseValue(float value) => Localizer.Do($"Base value: {Text.Num(value)}");
        public LocString BaseValue(int value) => Localizer.Do($"Base value: {Text.Num(value)}");
        public LocString Multiplier(string multiplierName, float multiplier) => Localizer.Do($"{multiplierName}: {Text.StyledPercent(multiplier - 1)}");
    }
}
