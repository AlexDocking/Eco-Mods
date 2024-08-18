using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public class EnsureValueIsAtLeast : IValueModifier<float>
    {
        public EnsureValueIsAtLeast(float minimum)
        {
            Minimum = minimum;
        }

        public float Minimum { get; }

        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            if (functionInput.Input < Minimum)
            {
                return new OverwriteModificationOutput(Minimum, Localizer.DoStr($"Must be at least {Text.Num(Minimum)} (got {Text.Num(functionInput.Input)})"));
            }
            return null;
        }
    }
}
