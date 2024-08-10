using Eco.Shared.Localization;
using Eco.Shared.Utils;
using EcompatibleTools;

namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IValueModifier
    {
        public float Priority => float.MaxValue;
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            Log.WriteLine(Localizer.Do($"Fallback input:{context.FloatValue},{context.IntValue},{shovelContext.TargetItem}"));
            if (context.FloatValue > 0) return;
            if (shovelContext.TargetItem == null) return;
            int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, 0, shovelContext.User);
            context.FloatValue = maxAccepted;
            context.IntValue = maxAccepted;
            Log.WriteLine(Localizer.Do($"Fallback output:{maxAccepted}"));
            operationDetails = new BaseLevelOperationDetails("Carry Limit");
        }
    }
}
