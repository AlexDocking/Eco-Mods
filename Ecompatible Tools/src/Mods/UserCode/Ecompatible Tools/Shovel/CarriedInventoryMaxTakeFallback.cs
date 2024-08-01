namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IModifyValueInPlaceHandler
    {
        public float Priority => float.MaxValue;
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (context.FloatValue > 0) return;
            if (shovelContext.TargetItem == null) return;
            int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, shovelContext.User.Inventory.Carried.TotalNumberOfItems(shovelContext.TargetItem), shovelContext.User);
            context.FloatValue = maxAccepted;
            context.IntValue = maxAccepted;
        }
    }
}
