using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.Organisms;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public static partial class ContextProperties
    {
        public static ContextKey User { get; } = new(typeof(User), nameof(User));
        public static ContextKey Shovel { get; } = new(typeof(ShovelItem), nameof(Shovel));
        public static ContextKey Axe { get; } = new(typeof(AxeItem), nameof(Axe));
        public static ContextKey Tree { get; } = new(typeof(TreeEntity), nameof(Tree));
        public static ContextKey SweepingHandsTalent { get; } = new(typeof(MiningSweepingHandsTalent), nameof(SweepingHandsTalent));
        public static ContextKey ItemToPutInInventory { get; } = new(typeof(Item), nameof(ItemToPutInInventory));
        public static ContextKey Inventory { get; } = new ContextKey(typeof(Inventory), nameof(Inventory));
    }
}