using Eco.Shared.Localization;

namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IValueModifier
    {
        public float Priority => float.MaxValue;
        public void ModifyValue(IValueModificationContext context, out LocString description, out ModificationType modificationType)
        {
            description = LocString.Empty;
            modificationType = ModificationType.None;
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (context.FloatValue > 0) return;
            if (shovelContext.TargetItem == null) return;
            int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, shovelContext.User.Inventory.Carried.TotalNumberOfItems(shovelContext.TargetItem), shovelContext.User);
            context.FloatValue = maxAccepted;
            context.IntValue = maxAccepted;
            description = DescriptionGenerator.Obj.BaseValue(maxAccepted);
            modificationType = ModificationType.BaseValue;
        }
    }
}
