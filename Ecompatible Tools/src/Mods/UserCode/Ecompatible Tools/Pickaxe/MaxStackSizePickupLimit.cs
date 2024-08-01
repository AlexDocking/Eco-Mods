namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IModifyValueInPlaceHandler
    {
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.Resource.MaxStackSize;
            context.IntValue = sweepingHandsContext.Resource.MaxStackSize;
        }
    }
}
