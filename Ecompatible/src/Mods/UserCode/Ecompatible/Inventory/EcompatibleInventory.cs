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
        public IPriorityValueResolver<float, IUserPutItemInInventoryContext> Carried { get; } = ValueResolverFactory.CreatePriorityResolver<float, IUserPutItemInInventoryContext>(
            (float.MinValue, new MaxStackSizePickupLimit<IUserPutItemInInventoryContext>()),
            (float.MaxValue, new UniqueItemStackSizeModifier<IUserPutItemInInventoryContext>()));
    }
}
