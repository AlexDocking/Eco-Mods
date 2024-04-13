// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using System.ComponentModel;
    using Eco.Gameplay.DynamicValues;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Gameplay.GameActions;
    using Eco.Shared.Serialization;
    using Eco.Core.Items;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Gameplay.Players;
    using Eco.Shared.SharedTypes;
    using Eco.Shared.Utils;
    using Eco.Shared.Math;
    using World = Eco.World.World;
    using Eco.World;
    using Eco.Gameplay.Placement;
    using Eco.World.Blocks;
    using Eco.Gameplay.Systems;
    using Eco.Gameplay.Utils;
    using System.Collections.Generic;
    using System.Collections;
    using System.Collections.Specialized;
    using CompatibleTools;
    using System;
    using Eco.Gameplay.Systems.NewTooltip;

    [Serialized]
    [LocDisplayName("Shovel")]
    [Weight(0)]
    [Category("Hidden"), Tag("Excavation"), Tag("Harvester")]
    [CanAirInteraction]
    public abstract partial class ShovelItem : ToolItem, IInteractor
    {
        private static SkillModifiedValue caloriesBurn      = CreateCalorieValue(20, typeof(SelfImprovementSkill), typeof(ShovelItem));
        private static IDynamicValue      skilledRepairCost = new ConstantValue(1);
        private static IDynamicValue      tier              = new ConstantValue(0);

        public override GameActionDescription      DescribeBlockAction               => GameActionDescription.DoStr("dig up", "digging up");
        public override IDynamicValue              CaloriesBurn                      => caloriesBurn; 
        public override IDynamicValue              Tier                              => tier;
        public override IDynamicValue              SkilledRepairCost                 => skilledRepairCost;
        public override int                        FullRepairAmount                  => 1;
        public override int                        MaxTake                           => 10;
        public override bool                       IsValidForInteraction(Item item)  => base.IsValidForInteraction(item) && (item?.GetType().HasTag(TagManager.GetTagOrFail("Diggable")) ?? false);
        public override                            ItemCategory ItemCategory         => ItemCategory.Shovel;

        /// <summary>
        /// List of modifiers that change MaxTake. Takes DigParams and current MaxTake, returns new MaxTake which is passed to the next function
        /// </summary>
        public static ICollection<IMaxTakeModifier> MaxTakeModifiers { get; } = new SortedCollection<IMaxTakeModifier>(new NumericComparer<IMaxTakeModifier>(modifier => modifier.Priority));
        private class SortedCollection<T> : ICollection<T>
        {
            public int Count => ((ICollection<T>)Values).Count;

            public bool IsReadOnly => ((ICollection<T>)Values).IsReadOnly;

            private List<T> Values { get; } = new List<T>();
            private IComparer<T> Comparer { get; }

            public SortedCollection(IComparer<T> comparer)
            {
                Comparer = comparer;
            }

            public void Add(T item)
            {
                ((ICollection<T>)Values).Add(item);
                Values.Sort(Comparer);
            }

            public void Clear()
            {
                ((ICollection<T>)Values).Clear();
            }

            public bool Contains(T item)
            {
                return ((ICollection<T>)Values).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                ((ICollection<T>)Values).CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)Values).GetEnumerator();
            }

            public bool Remove(T item)
            {
                return ((ICollection<T>)Values).Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Values).GetEnumerator();
            }
        }
        /// <summary>
        /// Calculate how much the shovel can dig. If no limit is imposed it will return null and it will be left to the player's inventory to decide whether to accept the block
        /// </summary>
        /// <param name="modification"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        public int? CalculateMaxTake(ShovelMaxTakeModification modification, IEnumerable<IMaxTakeModifier> modifiers)
        {
            foreach (var modifier in modifiers)
            {
                modifier.ModifyMaxTake(modification);
            }
            int maxTake = (int)Math.Floor(modification.MaxTake);
            if (maxTake > 0) return maxTake;
            if (modification.TargetItem == null) return null;
            int maxAccepted = modification.User.Inventory.Carried.GetMaxAcceptedVal(modification.TargetItem, modification.User.Inventory.Carried.TotalNumberOfItems(modification.TargetItem), modification.User);
            return maxAccepted;
        }
        [Interaction(InteractionTrigger.RightClick, canHoldToTrigger: TriBool.True, animationDriven: true, Priority = -1,
            RequiredEnvVars = new[] { ClientSideEnvVars.Carried }, Flags = InteractionFlags.MustNotHaveTarget)]
        public bool Drop(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
            => BlockPlacementUtils.DropCarriedBlock(player);

        [Interaction(InteractionTrigger.LeftClick, tags: BlockTags.Diggable, canHoldToTrigger: TriBool.True, animationDriven: true)]
        public bool Dig(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
        {
            // Fallback if not enough room in carrying stack
            var carry = player.User.Carrying;
            ShovelMaxTakeModification modification = new ShovelMaxTakeModification()
            {
                User = player.User,
                InteractionTriggerInfo = triggerInfo,
                InteractionTarget = target,
                TargetItem = target.Block()?.GetItem(),
                MaxTake = this.MaxTake,
                InitialMaxTake = this.MaxTake,
                Shovel = this
            };
            int maxTake = CalculateMaxTake(modification, MaxTakeModifiers) ?? -1;
            if (maxTake > 0 && carry.Quantity >= maxTake)
            {
                player.ErrorLoc($"Can't dig while carrying {player.User.Carrying.UILink()}.");
                return false;
            }

            //Find all plants we can harvest and all blocks we can dig within area of effect of this tool
            var anyPlants = this.TryCreateMultiblockContext(out var plantContext, target, player, tagsTargetable: BlockTags.Diggable, mustNotHaveTags: BlockTags.NonPlant.SingleItemAsEnumerable(), applyXPSkill: true);
            var anyOtherBlocks = this.TryCreateMultiblockContext(out var otherContext, target, player, tagsTargetable: BlockTags.Diggable, mustHaveTags: BlockTags.NonPlant.SingleItemAsEnumerable(), applyXPSkill: false);

            if (anyPlants || anyOtherBlocks)
            {
                using var pack = new GameActionPack();

                //Harvest all targeted plants
                if (anyPlants) pack.DestroyPlant(plantContext, harvestTo: player?.User.Inventory);
                //Destroy all targeted blocks that are not plants and harvest plants above them if those plants can be harvested by this tool
                if (anyOtherBlocks) pack.DeleteBlock(otherContext, player?.User.Inventory);

                return pack.TryPerform(player.User).Success;
            }

            return false;
        }
    }
}
