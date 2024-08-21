namespace Ecompatible
{
    internal sealed class MaxStackSizePickupLimit<TContext> : IValueModifier<float, TContext> where TContext : IPutItemInInventoryContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            var context = functionInput.Context;
            if (context.ItemToPutInInventory == null) return null;
            return OutputFactory.BaseLevel(context.ItemToPutInInventory.MaxStackSize);
        }
    }
}
