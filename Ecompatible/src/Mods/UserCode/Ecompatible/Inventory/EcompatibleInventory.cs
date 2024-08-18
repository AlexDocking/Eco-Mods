namespace Ecompatible
{
    public static partial class ValueResolvers
    {
        public static InventoryResolvers Inventory { get; } = new InventoryResolvers();
    }
    public partial class InventoryResolvers
    {
        public UserInventoryResolvers User { get; } = new UserInventoryResolvers();
    }
    public partial class UserInventoryResolvers
    {
        public IPriorityValueResolver<float> Carried { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new MaxStackSizePickupLimit()),
            (float.MaxValue, new UniqueItemStackSizeModifier()));
    }
}
