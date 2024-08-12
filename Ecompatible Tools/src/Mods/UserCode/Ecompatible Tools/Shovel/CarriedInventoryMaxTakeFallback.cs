namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (context is not ShovelMaxTakeModificationContext shovelContext) return null;
            if (functionInput.Input > 0) return null;
            if (shovelContext.TargetItem == null) return null;
            int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, 0, shovelContext.User);
            return new BaseLevelModificationOutput(maxAccepted, "Carry Limit");
        }
    }
}
