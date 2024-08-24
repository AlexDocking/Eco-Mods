// Parts of this file are:
// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.
//
// The remainder is:
// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.GameActions;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Players;
using Eco.Shared.SharedTypes;
using Eco.Shared.Utils;
using Eco.Gameplay.Utils;
using Ecompatible;
using Eco.Gameplay.Systems.NewTooltip;

namespace Eco.Mods.TechTree
{
    public abstract partial class ShovelItem
    {
        [ReplacementInteraction("Dig")]
        public bool EcompatibleDig(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
        {
            // Fallback if not enough room in carrying stack
            var carry = player.User.Carrying;
            IShovelPickUpContext maxTakeContext = ContextFactory.CreateShovelPickUpContext(
                user: player.User,
                itemToPutInInventory: target.Block()?.GetItem(),
                shovel: this
            );
            int maxTake = ValueResolvers.Tools.Shovel.MaxTakeResolver.ResolveInt(this.MaxTake, maxTakeContext);
            if (carry.Quantity >= maxTake)
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
