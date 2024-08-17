using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.Organisms;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public static partial class ContextProperties
    {
        public static ContextKey User { get; } = new(typeof(User), nameof(User));
        public static ContextKey TargetItem { get; } = new(typeof(Item), nameof(TargetItem));
        public static ContextKey Shovel { get; } = new(typeof(EcompatibleDig), nameof(Shovel));
        public static ContextKey Axe { get; } = new(typeof(AxeItem), nameof(Axe));
        public static ContextKey Tree { get; } = new(typeof(TreeEntity), nameof(Tree));
        public static ContextKey Resource { get; } = new(typeof(Item), nameof(Resource));
        public static ContextKey SweepingHandsTalent { get; } = new(typeof(MiningSweepingHandsTalent), nameof(SweepingHandsTalent));
    }
}