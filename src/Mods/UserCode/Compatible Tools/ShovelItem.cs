﻿// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.GameActions;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using Eco.Gameplay.Utils;
using CompatibleTools;
using Eco.Gameplay.Systems.NewTooltip;
using InteractionPatching;

namespace Eco.Mods.TechTree
{
    public abstract partial class ShovelItem
    {
        /// <summary>
        /// List of modifiers that change MaxTake.
        /// </summary>
        public static PriorityDynamicValueResolver MaxTakeResolver { get; } = new(new CarriedInventoryMaxTakeFallback());

        public class CarriedInventoryMaxTakeFallback : IPriorityModifyInPlaceDynamicValueHandler
        {
            public float Priority => float.MaxValue;
            public void ModifyValue(IModifyInPlaceDynamicValueContext context)
            {
                if (context is not ShovelMaxTakeModificationContext shovelContext) return;
                if (context.FloatValue > 0) return;
                if (shovelContext.TargetItem == null) return;
                Log.WriteLine(Shared.Localization.Localizer.Do($"Carry inv fallback"));
                int maxAccepted = shovelContext.User.Inventory.Carried.GetMaxAcceptedVal(shovelContext.TargetItem, shovelContext.User.Inventory.Carried.TotalNumberOfItems(shovelContext.TargetItem), shovelContext.User);
                context.FloatValue = maxAccepted;
                context.IntValue = maxAccepted;
            }
        }

        [ReplacementInteraction("Dig")]
        public bool ModifiedDig(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
        {
            // Fallback if not enough room in carrying stack
            var carry = player.User.Carrying;
            ShovelMaxTakeModificationContext maxTakeContext = new ShovelMaxTakeModificationContext()
            {
                User = player.User,
                TargetItem = target.Block()?.GetItem(),
                IntValue = this.MaxTake,
                FloatValue = this.MaxTake,
                Shovel = this
            };
            int maxTake = MaxTakeResolver.ResolveInt(maxTakeContext);
            Log.WriteLine(Shared.Localization.Localizer.Do($"DigOverride:{maxTake}"));
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