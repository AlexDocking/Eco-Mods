namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.Resource.MaxStackSize;
            operationDetails = new BaseLevelOperationDetails();
        }
    }
}
