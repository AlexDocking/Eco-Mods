using Eco.Gameplay.Items;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public class SweepingHandsMaxTakeModificationContext : ValueModificationContextBase
    {
        public Item Resource { get; init; }
        public MiningSweepingHandsTalent SweepingHandsTalent { get; init; }
    }
}
