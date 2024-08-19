using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.Organisms;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public static partial class ContextProperties
    {
        public static ContextKey<User> User { get; } = new(nameof(User));
        public static ContextKey<ShovelItem> Shovel { get; } = new(nameof(Shovel));
        public static ContextKey<ToolItem> ToolUsed { get; } = new(nameof(ToolUsed));
        public static ContextKey<TreeEntity> Tree { get; } = new(nameof(Tree));
        public static ContextKey<MiningSweepingHandsTalent> SweepingHandsTalent { get; } = new(nameof(SweepingHandsTalent));
        public static ContextKey<Item> ItemToPutInInventory { get; } = new(nameof(ItemToPutInInventory));
        public static ContextKey<Inventory> Inventory { get; } = new (nameof(Inventory));
    }
}