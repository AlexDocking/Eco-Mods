namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IValueModifier
    {
        public float Priority => float.MaxValue;
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not ShovelMaxTakeModificationContext shovelContext) return;
            if (context.FloatValue > 0) return;
            if (shovelContext.TargetItem == null) return;
            int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, 0, shovelContext.User);
            context.FloatValue = maxAccepted;
            operationDetails = new BaseLevelOperationDetails("Carry Limit");
        }
    }
}
