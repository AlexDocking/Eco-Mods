namespace CompatibleTools
{
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using Eco.Shared.SharedTypes;
    using System;

    public class ShovelMaxTakeModification
    {
        public User User { get; init; }
        public InteractionTriggerInfo? InteractionTriggerInfo { get; init; }
        public InteractionTarget? InteractionTarget { get; init; }
        public Item TargetItem { get; init; }
        public float MaxTake { get; set; }
        public float InitialMaxTake { get; init; }
        public ShovelItem Shovel { get; init; }
    }
}
