using Eco.Gameplay.Items;

namespace Ecompatible
{
    /// <summary>
    /// List of modifiers that change the max log pickup.
    /// </summary>
    /// <summary>
    /// Use the log's MaxStackSize as the default value
    /// </summary>
    public class MaxStackSizeLogPickupLimit : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (context is not TreeEntityMaxPickUpModificationContext treeContext) return null;
            var resourceType = treeContext.Tree.Species.ResourceItemType;
            var resource = Item.Get(resourceType);
            return new BaseLevelModificationOutput(resource.MaxStackSize);
        }
    }
}