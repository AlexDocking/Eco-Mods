using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using System.Numerics;

namespace Ecompatible
{
    public class SweepingHandsMaxTakeModificationContext : ValueModificationContextBase
    {
        public Item Resource { get; init; }
        public MiningSweepingHandsTalent SweepingHandsTalent { get; init; }
        public Vector3 Position => User.Position;
    }
}
