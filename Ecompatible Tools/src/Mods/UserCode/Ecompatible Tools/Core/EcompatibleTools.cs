namespace Ecompatible
{
    public static partial class ValueResolvers
    {
        public static ToolResolvers Tools { get; } = new ToolResolvers();
    }
    public partial class ToolResolvers
    {
        public ShovelResolvers Shovel { get; } = new ShovelResolvers();
        public PickaxeResolvers Pickaxe { get; } = new PickaxeResolvers();
        public AxeResolvers Axe { get; } = new AxeResolvers();
    }
    public partial class ShovelResolvers
    {
        /// <summary>
        /// List of modifiers that change MaxTake.
        /// </summary>
        public IPriorityValueResolver MaxTakeResolver { get; } = new PriorityDynamicValueResolver((float.MaxValue, new CarriedInventoryMaxTakeFallback()));
    }
    public partial class PickaxeResolvers
    {
        public IPriorityValueResolver MaxStackSizeResolver { get; } = new PriorityDynamicValueResolver((float.MinValue, new MaxStackSizePickupLimit()));
        public MiningSweepingHandsResolvers MiningSweepingHands { get; } = new MiningSweepingHandsResolvers();
    }
    public partial class MiningSweepingHandsResolvers
    {
        public IPriorityValueResolver PickUpRangeResolver { get; } = new PriorityDynamicValueResolver((float.MinValue, new DefaultPickupRange()));
    }
    public partial class AxeResolvers
    {
        public IPriorityValueResolver MaxPickupLogsResolver { get; } = new PriorityDynamicValueResolver((float.MinValue, new MaxStackSizeLogPickupLimit()));
    }
}