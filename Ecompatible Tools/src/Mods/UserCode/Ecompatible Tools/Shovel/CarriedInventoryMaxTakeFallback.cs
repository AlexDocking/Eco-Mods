using Eco.Gameplay.Items;
using Eco.Gameplay.Players;

namespace Ecompatible
{
    public class CarriedInventoryMaxTakeFallback : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.TryGetNonNull(ContextProperties.User, out User user)) return null;
            if (!context.TryGetNonNull(ContextProperties.TargetItem, out Item targetItem)) return null;
            if (functionInput.Input > 0) return null;
            int maxAccepted = user.Inventory.Carried.GetMaxAcceptedVal(targetItem, 0, user);
            return new BaseLevelModificationOutput(maxAccepted, "Carry Limit");
        }
    }
}
