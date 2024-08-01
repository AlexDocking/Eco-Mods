using Eco.Shared.Localization;

namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, out LocString description)
        {
            description = LocString.Empty
            if (context is not SweepingHandsMaxTakeModificationContext sweepingHandsContext) return;
            context.FloatValue = sweepingHandsContext.Resource.MaxStackSize;
            context.IntValue = sweepingHandsContext.Resource.MaxStackSize;
            description = DescriptionGenerator.Obj.BaseValue(context.IntValue);
        }
    }
}
