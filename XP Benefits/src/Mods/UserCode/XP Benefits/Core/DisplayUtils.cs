using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace XPBenefits
{
    internal static class DisplayUtils
    {
        public static string GradientNum(float v, object text, Eco.Shared.Math.Range range)
        {
            float percent = range.PercentThrough(v);
            if (percent < 0.5)
            {
                return Text.Color(Color.Red.Lerp(Color.Yellow, percent * 2), text);
            }
            else
            {
                return Text.Color(Color.Yellow.Lerp(Color.Green, percent / 2), text);
            }
        }
        public static LocString GradientNumLoc(float v, object text, Eco.Shared.Math.Range range) => Localizer.DoStr(GradientNum(v, text, range));

    }
}