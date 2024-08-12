namespace Ecompatible
{
    public class DefaultPickupRange : IValueModifier
    {
        public IModificationOutput ModifyValue(IModificationInput functionInput)
        {
            var context = functionInput.Context;
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return null;
            return new BaseLevelModificationOutput(sweepingHandsContext.SweepingHandsTalent.PickUpRange);
        }
    }
}
