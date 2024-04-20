// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Items;
using Eco.Gameplay.GameActions;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using Eco.Gameplay.Utils;
using System.Collections.Generic;
using CompatibleTools;
using System;
using Eco.Gameplay.Systems.NewTooltip;
using InteractionPatching;

namespace Eco.Mods.TechTree
{
    public abstract partial class ShovelItem
    {
        /// <summary>
        /// List of modifiers that change MaxTake. Takes DigParams and current MaxTake, returns new MaxTake which is passed to the next function
        /// </summary>
        public static ICollection<IMaxTakeModifier> MaxTakeModifiers { get; } = new SortedCollection<IMaxTakeModifier>(new NumericComparer<IMaxTakeModifier>(modifier => modifier.Priority));

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
        
        [ReplacementInteraction(nameof(Dig))]
        public bool DigOverride(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
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
