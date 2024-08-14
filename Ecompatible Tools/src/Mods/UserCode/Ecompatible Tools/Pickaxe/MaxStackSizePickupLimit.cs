using Eco.Gameplay.Items;

namespace Ecompatible
{
    public class MaxStackSizePickupLimit : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            var context = functionInput.Context;
            if (!context.HasProperty(ContextProperties.Resource, out Item resource)) return null;
            return new BaseLevelModificationOutput(resource.MaxStackSize);
        }
    }
}
