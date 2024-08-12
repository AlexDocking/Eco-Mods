namespace Ecompatible
{
    public class DefaultPickupRange : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return null;
            return new BaseLevelModificationOutput(sweepingHandsContext.SweepingHandsTalent.PickUpRange);
        }
    }
}
