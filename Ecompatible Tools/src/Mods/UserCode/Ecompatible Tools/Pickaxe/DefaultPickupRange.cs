using Eco.Mods.TechTree;

namespace Ecompatible
{
    public class DefaultPickupRange : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.HasProperty(ContextProperties.SweepingHandsTalent, out MiningSweepingHandsTalent sweepingHands)) return null;
            return new BaseLevelModificationOutput(sweepingHands.PickUpRange);
        }
    }
}
