using Eco.Gameplay.Items;
using Eco.Mods.Organisms;

namespace Ecompatible
{
    /// <summary>
    /// List of modifiers that change the max log pickup.
    /// Use the log's MaxStackSize as the default value
    /// </summary>
    public class MaxStackSizeLogPickupLimit : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.TryGetNonNull(ContextProperties.Tree, out TreeEntity tree)) return null;
            var resourceType = tree.Species.ResourceItemType;
            var resource = Item.Get(resourceType);
            return new BaseLevelModificationOutput(resource.MaxStackSize);
        }
    }
}