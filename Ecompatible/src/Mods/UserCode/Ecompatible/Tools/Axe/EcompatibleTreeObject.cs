// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.Organisms
{

    using System;
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
    using static Eco.Gameplay.Civics.IfThenBlock;
    using Eco.Gameplay.DynamicValues;
    public partial class TreeEntity
    {
        /// <summary>Max number of tree debris spawn from the tree.</summary>
        private int MaxTreeDebris { get; set; }
        private float ChanceToClearDebrisOnSpawn { get; set; }
        public float StumpHealth => this.stumpHealth;
        private Item ToolUsedToFellTree { get; set; }
        
        [Interaction(InteractionTrigger.InteractKey, requiredEnvVars: new[] { "canPickup", "id" }, animationDriven: true)]                        //A definition for when we can actually pickup
        public void PickUp(Player player, InteractionTriggerInfo trigger, InteractionTarget target) 
        { 
            if (target.TryGetParameter("id", out var id)) 
                this.EcompatiblePickupLog(player, (Guid) id, target.HitPos); 
        }
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
                                var context = ContextFactory.CreateUserPutItemInInventoryContext(
                                    user: player.User,
                                    itemToPutInInventory: Item.Get(Species.ResourceItemType)
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

        /// <summary>
        /// Process the tree if it hasn't been done yet
        /// </summary>
        private void ForceProcessing()
        {
            DoAutomaticProcessing(UserManager.FindUserByID(this.ChopperUserID), ToolUsedToFellTree);
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
                    ToolUsedToFellTree = tool;
                    this.FellTree(damager);
                    this.ChopperUserID = damager is Player player ? player.User.Id : -1;
                    EcoSim.PlantSim.KillPlant(this, DeathType.Logging, true);
                    CallbackSchedulerPlugin.QueueCallback(ForceProcessing, 8000);
                }

                this.MarkDirty();
            });
            return pack;
        }

        private bool processed = false;
        private object processingLock = new object();
        /// <summary>
        /// Slice up tree and damage stump according to resolver values
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tool"></param>
        /// 
        private void DoAutomaticProcessing(User user, Item tool)
        {
            if (processed) return;
            lock (processingLock)
            {
                if (processed) return;
                ITreeFelledContext context = ContextFactory.CreateTreeFelledContext(
                    user: user,
                    axe: tool as AxeItem,
                    tree: this
                    );
                float fractionOfTreeToSlice = ValueResolvers.Tools.Axe.FractionOfTreeToAutoSlice.Resolve(0, context);

                AutoSliceTrunk(user.Player, tool, fractionOfTreeToSlice);

                MaxTreeDebris = ValueResolvers.Tools.Axe.MaxTreeDebrisToSpawn.ResolveInt(MaxTreeDebris, context);
                ChanceToClearDebrisOnSpawn = ValueResolvers.Tools.Axe.ChanceToClearDebrisOnSpawn.Resolve(0, context);
                
                float amountToDamageStump = ValueResolvers.Tools.Axe.DamageToStumpWhenFelled.Resolve(0, context);

                //The trunk doesn't have a valid position when it is created, and clearing the stump tries to destroy the tree if there are no pieces with valid y positions
                Vector3 startPosition = default;
                if (this.trunkPieces.TryGetSingle(out var piece) && !piece.IsValid)
                {
                     startPosition = piece.Position;
                }
                TryDamageStump(new GameActionPack(), user?.Player, amountToDamageStump, tool).TryPerform(user);
                if (piece != null) piece.Position = startPosition;

                processed = true;
            }
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
            
            ThreadSafeList<TrunkPiece> pieces = this.trunkPieces;
            
            float fractionOfTreeSliced = 0;
            while (pieceNum < pieces.Count && fractionOfTreeSliced < fractionOfTreeToSlice)
            {
                var pieceToSlice = pieces[pieceNum];
                var slicePoint = pieceToSlice.SliceStart + 0.001f; //Slice a tiny amount. This is smaller than the actual slice will be to ensure it cannot be cut further.
                
                using (var pack = new GameActionPack())
                {
                    if (tool is ToolItem toolItem) pack.UseTool(toolItem.CreateMultiblockContext(player, false, pieceToSlice.Position.XYZi()));
                    if (!this.TrySliceTrunk(pack, damager, float.MaxValue, slicePoint, tool).TryPerform(player?.User)) return;
                }
                fractionOfTreeSliced = pieceToSlice.SliceEnd;
                pieceNum++;
            }
        }

        [RPC]
        public void CollideWithTerrain(Player player, Vector3i position)
        {
            if (player != this.Controller)
                return;

            DoAutomaticProcessing(player.User, ToolUsedToFellTree);

            lock (this.sync)
            {
                if (this.groundHits == null)
                    this.groundHits = new ThreadSafeHashSet<Vector3i>();
            }

            // Prevent spawning more than MaxGroundHits debris for one tree
            if (this.treeDebrisSpawned >= MaxTreeDebris)
                return;

            // destroy plants and spawn dirt within a radius under the hit position
            var radius = 1;
            for (var x = -radius; x <= radius; x++)
                for (var z = -radius; z <= radius; z++)
                {
                    var offsetpos = position + new Vector3i(x, -1, z);
                    if (!this.groundHits.Add(offsetpos))
                        continue;

                    var abovepos = offsetpos + Vector3i.Up;
                    var aboveblock = World.GetBlock(abovepos);
                    var hitblock = World.GetBlock(offsetpos);
                    if (!aboveblock.Is<Solid>())
                    {
                        // turn soil into dirt
                        if (hitblock.GetType() == typeof(GrassBlock) || hitblock.GetType() == typeof(ForestSoilBlock))
                        {
                            player.SpawnBlockEffect(offsetpos, typeof(DirtBlock), BlockEffect.Delete);
                            World.SetBlock<DirtBlock>(offsetpos);
                            BiomePusher.AddFrozenColumn(offsetpos.XZ);
                        }

                        // kill any above plants
                        if (aboveblock is PlantBlock)
                        {
                            // make sure there is a plant here, sometimes world/ecosim are out of sync
                            var plant = EcoSim.PlantSim.GetPlant(abovepos);
                            if (plant != null)
                            {
                                player.SpawnBlockEffect(abovepos, aboveblock.GetType(), BlockEffect.Delete);
                                EcoSim.PlantSim.DestroyPlant(plant, DeathType.Logging, true, player.User);
                            }
                            else World.DeleteBlock(abovepos);
                        }

                        if (hitblock.Is<Solid>() && World.GetBlock(abovepos).Is<Empty>() && RandomUtil.Value < this.Species.ChanceToSpawnDebris)
                        {
                            //Attempt to chop the debris before it spawns to account for tool usage, calories and experience. If that fails, then it can spawn as normal
                            bool clearedDebrisBeforeSpawn = false;
                            if (RandomUtil.Value < this.ChanceToClearDebrisOnSpawn)
                            {
                                clearedDebrisBeforeSpawn = ClearUnspawnedDebris(player.User, ToolUsedToFellTree as ToolItem, abovepos);
                            }
                            if (!clearedDebrisBeforeSpawn)
                            {
                                GameActionAccumulator.Obj.AddGameActions(new CreateTreeDebris()
                                {
                                    Count = 1,
                                    ActionLocation = abovepos,
                                    Citizen = player.User
                                }, player?.User);

                                World.SetBlock(this.Species.DebrisType, abovepos);
                                player.SpawnBlockEffect(abovepos, this.Species.DebrisType, BlockEffect.Place);
                                RoomData.QueueRoomTest(abovepos);
                            }
                            if (Interlocked.Increment(ref this.treeDebrisSpawned) >= MaxTreeDebris) return;
                        }
                    }
                }
        }

        private bool ClearUnspawnedDebris(User user, ToolItem tool, Vector3i position)
        {
            MultiblockActionContext multiblockContext = tool?.CreateMultiblockContext(user.Player, false, position) ?? default;
            using (var pack = new GameActionPack()) //Create game action pack, compose and try to perform
            {
                //Add debris items to inventory
                foreach (var x in this.Species.DebrisResources)
                    pack.AddToInventory(user.Inventory, Item.Get(x.Key), x.Value.RandInt, user);

                if (tool != null)
                {
                    //Set description and reduce XP multiplier for cleaning debris
                    multiblockContext.ActionDescription = GameActionDescription.DoStr("clean up tree debris", "cleaning up tree debris");
                    multiblockContext.ExperiencePerAction *= 0.1f;
                    pack.UseTool(multiblockContext);
                }
                bool success = !pack.TryPerform(user).Failed;   //Return true on success and false on failure
                return success;
            }
        }
    }
}
