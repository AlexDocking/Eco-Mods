using Eco.Gameplay.Items;

namespace Ecompatible
{
    /// <summary>
    /// List of modifiers that change the max log pickup.
    /// </summary>
    /// <summary>
    /// Use the log's MaxStackSize as the default value
    /// </summary>
    public class MaxStackSizeLogPickupLimit : IModifyValueInPlaceHandler
    {
        public void ModifyValue(IModifyValueInPlaceContext context)
        {
            if (context is not TreeEntityMaxPickUpModificationContext treeContext) return;
            var resourceType = treeContext.Tree.Species.ResourceItemType;
            var resource = Item.Get(resourceType);
            context.FloatValue = context.IntValue = resource.MaxStackSize;
        }
    }
}