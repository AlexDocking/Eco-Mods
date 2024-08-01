using Eco.Shared.Localization;

namespace Ecompatible
{
    public class DefaultPickupRange : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, out LocString description)
        {
            description = LocString.Empty;
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
            context.IntValue = sweepingHandsContext.SweepingHandsTalent.PickUpRange;
            description = DescriptionGenerator.Obj.BaseValue(context.FloatValue);
        }
    }
}
