using Eco.Gameplay.Items;

namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.TryGetNonNull(ContextProperties.ItemToPutInInventory, out Item item)) return null;
            return Output.BaseLevel(item.MaxStackSize);
        }
    }
}
