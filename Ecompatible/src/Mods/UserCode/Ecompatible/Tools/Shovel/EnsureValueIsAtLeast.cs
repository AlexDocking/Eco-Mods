using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    internal class EnsureValueIsAtLeast<TContext> : IValueModifier<float, TContext> where TContext : IContext
    {
        public EnsureValueIsAtLeast(float minimum)
        {
            Minimum = minimum;
        }

        public float Minimum { get; }

        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            if (functionInput.Input < Minimum)
            {
                return OutputFactory.Overwrite(Minimum, Localizer.DoStr($"Must be at least {Text.Num(Minimum)} (got {Text.Num(functionInput.Input)})"));
            }
            return null;
        }
    }
}
