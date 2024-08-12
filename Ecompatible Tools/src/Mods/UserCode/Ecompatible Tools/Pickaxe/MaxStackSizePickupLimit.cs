namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IValueModifier
    {
        public IModificationOutput ModifyValue(IModificationInput functionInput)
        {
            var context = functionInput.Context;
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return null;
            return new BaseLevelModificationOutput(sweepingHandsContext.Resource.MaxStackSize);
        }
    }
}
