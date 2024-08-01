namespace Ecompatible
{
    public class DefaultPickupRange : IModifyValueInPlaceHandler
    {
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
            context.IntValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
        }
    }
}
