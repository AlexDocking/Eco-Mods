using Eco.Gameplay.Items;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public class ShovelMaxTakeModificationContext : ValueModificationContextBase
    {
        public Item TargetItem { get; init; }
        public ShovelItem Shovel { get; init; }
    }
}
