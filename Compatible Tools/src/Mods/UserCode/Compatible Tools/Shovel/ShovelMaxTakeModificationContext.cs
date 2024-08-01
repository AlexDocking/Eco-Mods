namespace EcompatibleTools
{
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using Eco.Shared.SharedTypes;
    using System;
    using System.Numerics;

    public class ShovelMaxTakeModificationContext : IModifyValueInPlaceContext
    {
        public User User { get; init; }
        public Item TargetItem { get; init; }
        public ShovelItem Shovel { get; init; }
        public Vector3 Position => User.Position;
        public float FloatValue { get; set; }
        public int IntValue { get; set; }
    }
}
