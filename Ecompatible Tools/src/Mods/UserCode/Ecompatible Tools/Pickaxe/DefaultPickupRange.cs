using Eco.Shared.Localization;
using EcompatibleTools;

namespace Ecompatible
{
    public class DefaultPickupRange : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
            context.IntValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
            operationDetails = new BaseLevelOperationDetails();
        }
    }
}
