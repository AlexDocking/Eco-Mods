namespace Ecompatible
{
    using Eco.Gameplay.Players;
    using Eco.Mods.Organisms;
    using Eco.Mods.TechTree;
    using System.Numerics;

    public class TreeEntityMaxPickUpModificationContext : IValueModificationContext
    {
        public User User { get; init; }
        public TreeEntity Tree { get; init; }
        public float InitialPickup { get; init; }
        public AxeItem Axe { get; init; }
        public Vector3 Position => User.Position;
        public float FloatValue { get; set; }
        public int IntValue { get; set; }
    }
}
