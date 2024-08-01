namespace EcompatibleTools
{
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using System.Numerics;

    public class SweepingHandsMaxTakeModificationContext : IModifyValueInPlaceContext
    {
        public User User { get; init; }
        public Item Resource { get; init; }
        public MiningSweepingHandsTalent SweepingHandsTalent { get; init; }
        public Vector3 Position => User.Position;
        public float FloatValue { get; set; }
        public int IntValue { get; set; }
    }
}
