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
        public IPriorityValueResolver<float> MaxTakeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MaxValue, new EnsureValueIsAtLeast(1)));
    }
    public partial class PickaxeResolvers
    {
        public MiningSweepingHandsResolvers MiningSweepingHands { get; } = new MiningSweepingHandsResolvers();
    }
    public partial class MiningSweepingHandsResolvers
    {
        public IPriorityValueResolver<float> PickUpRangeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new DefaultPickupRange()));
    }
    public partial class AxeResolvers
    {
        public IPriorityValueResolver<float> FractionOfTreeToAutoSlice { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new BaseLevelModifier(0)));
        public IPriorityValueResolver<float> DamageToStumpWhenFelled { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new BaseLevelModifier(0)));
        public IPriorityValueResolver<float> MaxTreeDebrisToSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new BaseLevelModifier(20)));
        public IPriorityValueResolver<float> ChanceToClearDebrisOnSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float>((float.MinValue, new BaseLevelModifier(0)));
    }
}