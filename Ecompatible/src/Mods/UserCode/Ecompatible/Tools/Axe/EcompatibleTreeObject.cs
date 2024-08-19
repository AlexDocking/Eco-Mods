// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.Organisms
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Eco.Core.Utils;
    using Eco.Core.Items;
    using Eco.Gameplay.Interactions;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Minimap;
    using Eco.Gameplay.Plants;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Rooms;
    using Eco.Gameplay.Systems.TextLinks;
    using Eco.Shared;
    using Eco.Shared.Math;
    using Eco.Shared.Networking;
    using Eco.Shared.Serialization;
    using Eco.Shared.Utils;
    using Eco.Shared.SharedTypes;
    using Eco.Simulation;
    using Eco.Simulation.Agents;
    using Eco.Simulation.Types;
    using Eco.World;
    using Eco.World.Blocks;
    using Eco.Gameplay.GameActions;
    using Eco.Simulation.WorldLayers.Pushers;
    using Eco.Shared.Localization;
    using Eco.Shared.Items;
    using Eco.Gameplay.Civics;
    using Vector3 = System.Numerics.Vector3;
    using System.ComponentModel;
    using Eco.Gameplay.Interactions.Interactors;
    using Eco.Shared.Time;
    using Ecompatible;
    using Eco.Shared.Logging;
    using Eco.Mods.TechTree;

    public partial class TreeEntity
    {
        void EcompatiblePickupLog(Player player, Guid logID, Vector3 pickupPosition)
        {
            lock (this.sync)
            {
                if (!this.CanHarvest)
                    player.ErrorLocStr("Log is not ready for harvest.  Remove all branches first.");

                var trunk = this.trunkPieces.FirstOrDefault(p => p.ID == logID);
                if (trunk?.IsCollectedOrNotValid == false)
                {
                    //split only pieces that are at least 5. Pieces are no larger than 5
                    //pick up only pieces that are no larger than 5, meaning pieces that cannot be split further
                    //
                    //Check log size, if its too big, it can't be picked up
                    var canPickup = this.GetBasePickupSize(trunk) <= MaxTrunkPickupSize;
                    if (!canPickup)
                    {
                        player.ErrorLocStr("Log is too large to pick up, slice into smaller pieces first.");
                        return;
                    }

                    var resourceType = this.Species.ResourceItemType;
                    var resource     = Item.Get(resourceType);
                    var baseCount    = this.GetBasePickupSize(trunk);
                    var yield        = resource.Yield;
                    var bonusItems   = yield?.GetCurrentValueInt(player.User.DynamicValueContext, null) ?? 0;
                    var numItems     = baseCount + bonusItems;
                    var carried      = player.User.Inventory.Carried;

                    if (numItems > 0)
                    {
                        if (!carried.IsEmpty) // Early tests: neeed to check type mismatch and max quantity.
                        {
                            if      (carried.Stacks.First().Item.Type != resourceType)                    { player.Error(Localizer.Format("You are already carrying {0:items} and cannot pick up {1:items}.", carried.Stacks.First().Item.UILink(LinkConfig.ShowPlural), resource.UILink(LinkConfig.ShowPlural)));  return; }                        
                            // Ecompatible Tools - Start
                            else
                            {
                                var context = Context.CreateContext(
                                    (ContextProperties.User, player.User),
                                    (ContextProperties.ItemToPutInInventory, Item.Get(Species.ResourceItemType))
                                    );
                                int maxStackSize = ValueResolvers.Inventory.User.Carried.ResolveInt(0, context);
                                if (carried.Stacks.First().Quantity + numItems > maxStackSize) { player.Error(Localizer.Format("You can't carry {0:n0} more {1:items} ({2} max).", numItems, resource.UILink(numItems != 1 ? LinkConfig.ShowPlural : 0), maxStackSize)); return; }
                            }
                            //Ecompatible Tools - Finish
                        }

                        // Prepare a game action pack.
                        var pack = new GameActionPack();
                            pack.AddPostEffect          (() => { trunk.Collected = true; this.RPC("DestroyLog", logID); this.MarkDirty(); this.CheckDestroy(); }); // Delete the log if succseeded.
                            pack.GetOrCreateInventoryChangeSet   (carried, player.User).AddItemsNonUnique(this.Species.ResourceItemType, numItems);                         // Add items to the changeset.
                            pack.AddGameAction          (new HarvestOrHunt() {   Species         = this.Species.GetType(),
                                                                                 HarvestedStacks = new ItemStack(Item.Get(this.Species.ResourceItemType), numItems).SingleItemAsEnumerable(),
                                                                                 ActionLocation  = pickupPosition.XYZi(),
                                                                                 Citizen         = player.User,
                                                                                 ChopperUserID   = this.ChopperUserID});                  
                            pack.TryPerform(player.User); // Try to perform the action and apply changes & effects.
                    }
                }
            }
        }
        
        private GameActionPack EcompatibleTryDamageTrunk(GameActionPack pack, INetObject damager, float amount, Item tool)
        {
            var user = (damager as Player)?.User;
            if (this.health <= 0) return pack;

            pack.AddGameAction(this.CreateChopTreeAction(damager, tool, this.health <= amount));

            pack.AddPostEffect(() =>
            { 
                // damage trunk
                var damageDone = InterlockedUtils.SubMinNonNegative(ref this.health, amount);
                if (damageDone <= 0f) return;

                this.RPC("UpdateHP", this.health / this.Species.TreeHealth);

                if (this.health <= 0)
                {
                    this.health = 0;
                    this.FellTree(damager);
                    this.ChopperUserID = damager is Player player ? player.User.Id : -1;
                    EcoSim.PlantSim.KillPlant(this, DeathType.Logging, true);

                    IContext context = Context.CreateContext(
                        (ContextProperties.User, (damager as Player)?.User),
                        (ContextProperties.Tree, this),
                        (ContextProperties.ToolUsed, tool as ToolItem)
                        );
                    float fractionOfTreeToSlice = ValueResolvers.Tools.Axe.FractionOfTreeToSliceWhenFelled.Resolve(0, context);

                    AutoSliceTrunk(damager as Player, tool, fractionOfTreeToSlice);
                }
                
                this.MarkDirty();
            });
            return pack;
        }
        
        /// <summary>
        /// Slice off small pieces of the trunk until the piece remaining is smaller than the fraction allowed
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tool"></param>
        /// <param name="fractionOfTreeToSlice">Slice until the fraction of the whole tree that has been sliced is larger than this</param>
        public void AutoSliceTrunk(Player player, Item tool, float fractionOfTreeToSlice)
        {
            var damager = player;

            int pieceNum = 0;
            
            float PieceSize(TrunkPiece piece)
            {
                return piece.SliceEnd - piece.SliceStart;
            }
            TrunkPiece GetLargestPiece()
            {
                var largestPiece = this.trunkPieces[0];
                for (int i = 0; i < this.trunkPieces.Count; i++)
                {
                    if (PieceSize(trunkPieces[i]) > PieceSize(largestPiece))
                    {
                        largestPiece = this.trunkPieces[i];
                    }
                }
                return largestPiece;
            }
            var pieceToSlice = GetLargestPiece();
            while (pieceNum < this.trunkPieces.Count && PieceSize(pieceToSlice) > (1 - fractionOfTreeToSlice))
            {
                pieceNum++;

                var slicePoint = pieceToSlice.SliceStart + 0.001f; //Slice a tiny amount. This is smaller than the actual slice will be to ensure it cannot be cut further.

                //If there are still branches, damage them instead
                for (var branchID = 0; branchID < this.branches.Length; branchID++)
                {
                    var branch = this.branches[branchID];
                    if (branch != null && branch.Health > 0)
                    {
                        branch.Health = 0;
                        this.RPC("DestroyBranch", branchID);
                        this.MarkDirty();
                    }
                }
                this.TrySliceTrunkInternal(slicePoint, damager);
                pieceToSlice = GetLargestPiece();
            }
        }
    }
}
