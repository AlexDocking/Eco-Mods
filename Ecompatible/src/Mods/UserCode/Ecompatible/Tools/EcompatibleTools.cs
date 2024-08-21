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
        public IPriorityValueResolver<float, IShovelPickUpContext> MaxTakeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float, IShovelPickUpContext>((float.MaxValue, new EnsureValueIsAtLeast<IShovelPickUpContext>(1)));
    }
    public partial class PickaxeResolvers
    {
        public MiningSweepingHandsResolvers MiningSweepingHands { get; } = new MiningSweepingHandsResolvers();
    }
    public partial class MiningSweepingHandsResolvers
    {
        public IPriorityValueResolver<float, IUserPickUpRubbleContext> PickUpRangeResolver { get; } = ValueResolverFactory.CreatePriorityResolver<float, IUserPickUpRubbleContext>((float.MinValue, new DefaultPickupRange<IUserPickUpRubbleContext>()));
    }
    public partial class AxeResolvers
    {
        public IPriorityValueResolver<float, ITreeFelledContext> FractionOfTreeToAutoSlice { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>((float.MinValue, new BaseLevelModifier<ITreeFelledContext>(0)));
        public IPriorityValueResolver<float, ITreeFelledContext> DamageToStumpWhenFelled { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>((float.MinValue, new BaseLevelModifier<ITreeFelledContext>(0)));
        public IPriorityValueResolver<float, ITreeFelledContext> MaxTreeDebrisToSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>((float.MinValue, new BaseLevelModifier<ITreeFelledContext>(20)));
        public IPriorityValueResolver<float, ITreeFelledContext> ChanceToClearDebrisOnSpawn { get; } = ValueResolverFactory.CreatePriorityResolver<float, ITreeFelledContext>((float.MinValue, new BaseLevelModifier<ITreeFelledContext>(0)));
    }
}