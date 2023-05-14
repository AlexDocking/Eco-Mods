// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eco.Core.Controller;
using Eco.Core.Utils;
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
using Eco.Simulation;
using Eco.Simulation.Agents;
using Eco.Simulation.Types;
using Eco.World;
using Eco.World.Blocks;
using Eco.Gameplay.GameActions;
using Eco.Simulation.WorldLayers.Pushers;
using Eco.Gameplay.Systems;
using Eco.Shared.Localization;
using Eco.Gameplay.Systems.Tooltip;
using Eco.Shared.Items;
using Eco.Gameplay.Civics;
using Vector3 = System.Numerics.Vector3;
using System.ComponentModel;

[Serialized]
class TrunkPiece
{
    [Serialized] public Guid ID;
    [Serialized] public float SliceStart;
    [Serialized] public float SliceEnd;

    public double LastUpdateTime;
    [Serialized] public Vector3 Position;
    [Serialized] public Vector3 Velocity;
    [Serialized] public Quaternion Rotation;

    [Serialized] public bool Collected;

    public BSONObject ToUpdateBson()
    {
        var bson = BSONObject.New;
        bson["id"] = this.ID;
        bson["pos"] = this.Position;
        bson["rot"] = this.Rotation;
        bson["v"]   = this.Velocity;
        return bson;
    }

    public BSONObject ToInitialBson()
    {
        var bson = BSONObject.New;
        bson["id"] = this.ID;
        bson["start"] = this.SliceStart;
        bson["end"] = this.SliceEnd;
        bson["pos"] = this.Position;
        bson["rot"] = this.Rotation;
        bson["v"]   = this.Velocity;
        bson["collected"] = this.Collected;
        return bson;
    }
}

// gameplay version of simulations tree
[Serialized] public class TreeEntity : Tree, IInteractableObject, IDamageable
{
    readonly object sync = new();

    public static float DamageExperienceMultiplier = 1f;
    /// <summary>This needs to be 5, because 5 is the max yield bonus, and 5+5=10 is the max log stack size.</summary>
    private const int MaxTrunkPickupSize = 5;
    /// <summary>Max number of tree debris spawn from the tree.</summary>
    private const int MaxTreeDebris = 20;
    /// <summary>The smallest difference in age before an update is sent.</summary>
    private const float AgeUpdateDifference = 0.1f;

    [Serialized] public Quaternion Rotation { get; protected set; }

    // the list of all the slices done to this trunk
    [Serialized] readonly ThreadSafeList<TrunkPiece> trunkPieces = new ThreadSafeList<TrunkPiece>();

    public override bool UpRooted => this.stumpHealth <= 0;

    public override float SaplingGrowthPercent => 0.3f;

    // estimated, need to get better measurements w/ and w/o Top branch
    private static readonly float[] GrowthThresholds = new float[7] { 0.20f, 0.28f, 0.38f, 0.48f, 0.57f, 0.78f, 0.95f };
    private int currentGrowthThreshold = 0;
    private int treeDebrisSpawned; // it is runtime variable, it shouldn't spawn from felt trees so should be fine, but even if hacked someway it will produce at max 10 debris after restart

    public double LastUpdateTime { get; private set; }
    float lastKeyframeTime;
    float lastGrowthPercent;

    [Serialized] public int ChopperUserID { get; protected set; } = -1;//who chopped down this tree have access to its trunks regardless of thier position

    public override IEnumerable<Vector3> TrunkPositions { get { return this.trunkPieces.Where(x => !x.Collected).Select(x => x.Position); } }

    private ThreadSafeHashSet<Vector3i> groundHits;
    MinimapObject minimapObject = new();

    public override bool Ripe
    {
        get
        {
            if (this.Fallen && this.trunkPieces.All(piece => piece.Collected))
                return false;
            return this.GrowthPercent >= this.SaplingGrowthPercent;
        }
    }

    public override bool GrowthBlocked
    {
        get
        {
            if (this.Fallen || this.GrowthPercent >= 1f)
                return true;
            if ((this.currentGrowthThreshold >= GrowthThresholds.Length) // already at max occupied spaces
                || this.GrowthPercent < GrowthThresholds[this.currentGrowthThreshold])
                return false;
            var block = World.GetBlock(this.Position.XYZi() + (Vector3i.Up * (this.currentGrowthThreshold + 1)));
            if (!block.Is<Empty>() && block.GetType() != this.Species.BlockType)
                    return true; // can't grow until obstruction is removed
            return false;
        }
    }

    public override float GrowthPercent
    {
        get => base.GrowthPercent;
        set { base.GrowthPercent = Mathf.Clamp01(value); this.lastGrowthPercent = this.GrowthPercent; this.UpdateGrowthOccupancy(); }
    }

    private void UpdateGrowthOccupancy()
    {
        if (this.Species == null || this.Fallen) return;

        WrappedWorldPosition3i position = this.Position.XYZi() + (Vector3i.Up * this.currentGrowthThreshold);

        while (this.currentGrowthThreshold < GrowthThresholds.Length && this.GrowthPercent >= GrowthThresholds[this.currentGrowthThreshold])
        {
            bool canGrow = position.TryIncreaseY(1, out position);

            if (canGrow)
            {
                // check if block is empty
                var block = World.GetBlock(position);
                if (!block.Is<Empty>() && block.GetType() != this.Species.BlockType) canGrow = false;
            }

            if (!canGrow)
            {
                base.GrowthPercent = GrowthThresholds[this.currentGrowthThreshold] - 0.01f;
                return; // cap growth at slightly less than threshold, can't grow until obstruction is removed
            }

            this.currentGrowthThreshold++;
            World.SetBlock(this.Species.BlockType, position);
            this.UpdateMinimapObjectScale(); //Update minimap tree icon only when growth threshold is reached.
        }
    }

    private bool CanHarvest => this.branches.None(branch => branch != null && branch.Health > 0f);  // can't harvest if any branches are still alive

    public INetObjectViewer Controller { get; private set; }

    #region IInteractable interface
    public float InteractDistance { get { return 2.0f; } }

    private bool CanPickup(TrunkPiece trunk)
    {
        return this.GetBasePickupSize(trunk) <= MaxTrunkPickupSize;
    }

    private float ResourceMultiplier => (this.Species.ResourceRange.Diff * this.GrowthPercent) + this.Species.ResourceRange.Min;

    private int GetBasePickupSize(TrunkPiece trunk) => Math.Max(Mathf.RoundUpToInt((trunk.SliceEnd - trunk.SliceStart) * this.ResourceMultiplier), 1);

    public InteractResult OnActLeft(InteractionContext context) { return InteractResult.NoOp; }
    public InteractResult OnActRight(InteractionContext context) { return InteractResult.NoOp; }
    public InteractResult OnPreInteractionPass(InteractionContext context) => InteractResult.NoOp;

    public InteractResult OnActInteract(InteractionContext context)
    {
        if (!this.Fallen)
            return InteractResult.NoOp;

        if (context.Parameters != null && context.Parameters.ContainsKey("id"))
        {
            this.PickupLog(context.Player, context.Parameters["id"], context.HitPosition.HasValue ? context.HitPosition.Value : context.TargetPosition);
            return InteractResult.Success;
        }

        return InteractResult.NoOp;
    }

    //False so it will create toggle at minimap ui to switch trees icons
    public bool IsOverlayObject => false;
#pragma warning disable CS0067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    #endregion

    #region IController
    int controllerID;
    private float nextGrowthUpdateThreshold = 0.0f;

    public ref int ControllerID => ref this.controllerID;
    #endregion

    public TreeEntity(TreeSpecies species, WorldPosition3i position, PlantPack plantPack)
        : base(species, position, plantPack)
    { }

    // needed for serialization
    protected TreeEntity()
    { }

    public override void Initialize()
    {
        base.Initialize();
        this.UpdateGrowthOccupancy();
        this.minimapObject.Position    = this.Position;
        this.minimapObject.Type        = this.Species.GetType();
        this.minimapObject.Category    = Localizer.DoStr("Trees");
        this.minimapObject.DisplayName = this.Species.DisplayName;

        this.UpdateMinimapObjectScale();
        if (!this.Fallen)
            MinimapManager.Obj.Objects.Add(this.minimapObject);
    }

    //The scale formula for the minimap is based on the Species's Scale and the Tree's Growth.
    void UpdateMinimapObjectScale()
    {
        if (this.Species == null || this.Fallen) return;

        float scaleXZ            = this.Species.XZScaleRange.Interpolate(this.scaleRandomValue); //Get a random XZ size based on the species range
        float scaleY             = this.Species.YScaleRange.Interpolate(this.scaleRandomValue);  //Get a random Y  size based on the species range
        this.minimapObject.Scale = new Vector3(scaleXZ, scaleY, scaleXZ) * Mathf.Lerp(0.25f, 1f, this.GrowthPercent); // Scale multiplied by growth percent (clamped min so newborn trees can be seen in minimap).
    }

    void CheckDestroy()
    {
        // destroy the tree if it has fallen, all the trunk pieces are collected, and the stump is removed
        if (this.Fallen && this.stumpHealth <= 0 && this.trunkPieces.All(piece => piece.Collected))
            this.Destroy();
    }

    void PickupLog(Player player, Guid logID, Vector3 pickupPosition)
    {
        lock (this.sync)
        {
            if (!this.CanHarvest)
                player.ErrorLocStr("Log is not ready for harvest.  Remove all branches first.");

            TrunkPiece trunk;
            trunk = this.trunkPieces.FirstOrDefault(p => p.ID == logID);
            if (trunk != null && trunk.Collected == false)
            {
                // check log size, if its too big, it can't be picked up
                if (!this.CanPickup(trunk))
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
                        if (carried.Stacks.First().Item.Type != resourceType) { player.Error(Localizer.Format("You are already carrying {0:items} and cannot pick up {1:items}.", carried.Stacks.First().Item.UILink(LinkConfig.ShowPlural), resource.UILink(LinkConfig.ShowPlural))); return; }
                        else
                        {
                            //Let the carry inventory decide how many logs it can hold, instead of using the default log stack size
                            int maxStackSize = carried.GetMaxAcceptedVal(resource, carried.Stacks.First().Quantity);
                            if (carried.Stacks.First().Quantity + numItems > maxStackSize) { player.Error(Localizer.Format("You can't carry {0:n0} more {1:items} ({2} max).", numItems, resource.UILink(numItems != 1 ? LinkConfig.ShowPlural : 0), maxStackSize)); return; }
                        }
                    }

                    // Prepare a game action pack.
                    var pack = new GameActionPack();
                        pack.AddPostEffect          (() => { trunk.Collected = true; this.RPC("DestroyLog", logID); this.MarkDirty(); this.CheckDestroy(); });  // Delete the log if succseeded.
                        pack.GetOrCreateInventoryChangeSet   (carried, player.User).AddItems(this.Species.ResourceItemType, numItems);                                   // Add items to the changeset.
                        pack.AddGameAction          (new HarvestOrHunt() {   Species            = this.Species.GetType(),
                                                                             HarvestedStacks    = new ItemStack(Item.Get(this.Species.ResourceItemType), numItems).SingleItemAsEnumerable(),
                                                                             ActionLocation     = pickupPosition.XYZi(),
                                                                             Citizen            = player.User,
                                                                             ChopperUserID      = this.ChopperUserID});                  
                        pack.TryPerform(); // Try to perform the action and apply changes & effects.
                }
            }
        }
    }

    #region RPCs
    [RPC]
    public void DestroyLeaf(int branchID, int leafID)
    {
        TreeBranch branch = this.branches[branchID];
        LeafBunch leaf = branch.Leaves[leafID];

        if (leaf.Health > 0)
        {
            // replicate to all clients
            leaf.Health = 0;
            this.RPC("DestroyLeaves", branchID, leafID);
        }
    }

    [RPC]
    public void DestroyBranch(int branchID)
    {
        TreeBranch branch = this.branches[branchID];

        if (branch.Health > 0)
        {
            // replicate to all clients
            branch.Health = 0;
            this.RPC("DestroyBranch", branchID);
        }

        this.MarkDirty();
    }

    TrunkPiece GetTrunkPiece(float slicePoint) => this.trunkPieces.FirstOrDefault(p => p.SliceStart < slicePoint && p.SliceEnd > slicePoint);

    private Result TrySliceTrunk(InteractionContext context)
    {
        lock (this.sync) // prevent threading issues due to multiple choppers
        {
            var player     = context.Player;
            var slicePoint = context.Parameters?["slice"].FloatValue ?? 0f;

            // find the trunk piece this is coming from
            var target = this.GetTrunkPiece(slicePoint);
            if (target == null) return Result.FailedNoMessage;

            // if this is a tiny slice, clamp to the nearest valid size
            const float minPieceResources = 5f;
            var         minPieceSize      = minPieceResources / this.ResourceMultiplier;
            var         targetSize        = target.SliceEnd - target.SliceStart;
            var         targetResources   = targetSize * this.ResourceMultiplier;
            var         newPieceSize      = target.SliceEnd - slicePoint;
            var         newPieceResources = newPieceSize * this.ResourceMultiplier;
            if (targetResources <= minPieceResources) return Result.FailLocStr("This log cannot be sliced any smaller"); // can't slice, too small

            if (targetResources < (2 * minPieceResources))           slicePoint = target.SliceStart + (.5f * targetSize);   // if smaller than 2x the min size, slice directly in half
            else if (newPieceSize < minPieceSize)                    slicePoint = target.SliceEnd - minPieceSize;           // round down to nearest slice point where the resulting block will be the size of the log
            else if (slicePoint - target.SliceStart <= minPieceSize) slicePoint = target.SliceStart + minPieceSize;         // round up

            var sourceID = target.ID;
            // slice and assign new IDs (New piece is always the back end of the source piece)
            var newPiece = new TrunkPiece()
            {
                ID         = Guid.NewGuid(),
                SliceStart = slicePoint,
                SliceEnd   = target.SliceEnd,
                Position   = target.Position,
                Rotation   = target.Rotation,
            };
            this.trunkPieces.Add(newPiece);
            target.ID       = Guid.NewGuid();
            target.SliceEnd = slicePoint;

            // ensure the pieces are listed in order
            this.trunkPieces.Sort((a, b) => a.SliceStart.CompareTo(b.SliceStart));

            // reciprocate to clients
            this.RPC("SliceTrunk", slicePoint, sourceID, target.ID, newPiece.ID);

            PlantSimEvents.OnLogChopped.Invoke(player?.User);

            this.MarkDirty();
            return Result.Succeeded;
        }
    }

    [RPC]
    public void CollideWithTerrain(Player player, Vector3i position)
    {
        if (player != this.Controller)
            return;

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
                        GameActionAccumulator.Obj.AddGameActions(new CreateTreeDebris()
                        {
                            Count = 1,
                            ActionLocation = abovepos,
                            Citizen = player.User
                        }, player?.User);

                        World.SetBlock(this.Species.DebrisType, abovepos);
                        player.SpawnBlockEffect(abovepos, this.Species.DebrisType, BlockEffect.Place);
                        RoomData.QueueRoomTest(abovepos);
                        if (Interlocked.Increment(ref this.treeDebrisSpawned) >= MaxTreeDebris) return;
                    }
                }
            }
    }
    #endregion

    ChopTree CreateChopTreeAction(InteractionContext context, bool felled, bool branches = false) => new ChopTree()
    {
        Citizen          = context.Player?.User,
        Species          = this.Species.GetType(),
        ChopperUserID    = this.ChopperUserID,
        ActionLocation   = this.Position.XYZi(),
        AccessNeeded     = AccessType.FullAccess,
        Felled           = felled,
        BranchesTargeted = branches,
        GrowthPercent    = this.GrowthPercent * 100,
        ToolUsed         = context.SelectedItem
    };

    void FellTree(INetObject killer)
    {
        // create the initial trunk piece
        var trunkPiece = new TrunkPiece() { ID = Guid.NewGuid(), SliceStart = 0f, SliceEnd = 1f,  };

        // clear tree occupancy
        if (this.Species.BlockType != null)
        {
            var treeBlockCheck = this.Position.XYZi() + Vector3i.Up;
            while (World.GetBlock(treeBlockCheck).GetType() == this.Species.BlockType)
            {
                World.DeleteBlock(treeBlockCheck);
                treeBlockCheck += Vector3i.Up;
            }
        }

        this.trunkPieces.Add(trunkPiece);

        var player = killer as Player;
        if (player != null)
        {
            this.SetPhysicsController(player);                                   // set the killing player's client as the one in control of the physics of the tree. Handled by "FellTree".
            player.RPC("YellTimber");                                            // Issue sound effect.
        }

        this.RPC("FellTree", trunkPiece.ID, this.ResourceMultiplier);            // Fell the tree
        Animal.AlertNearbyAnimals(this.Position, 15f);                           // Alert nearby animals to aware about falling tree

        // break off any branches that are young
        for (var branchID = 0; branchID < this.branches.Length; branchID++)
        {
            var branch = this.branches[branchID];
            if (branch == null)
                continue;

            var branchAge = Mathf.Clamp01((float)((this.GrowthPercent - branch.SpawnAge) / (branch.MatureAge - branch.SpawnAge)));
            if (branchAge <= .5f)
                this.DestroyBranch(branchID);
        }

        if (player != null)
            PlantSimEvents.TreeFelledEvent.Invoke(player.User, this.Species);
        MinimapManager.Obj.Objects.Remove(this.minimapObject);

        this.MarkDirty();
    }

    public GameActionPack TryApplyDamage(GameActionPack pack, INetObject damager, float amount, InteractionContext context, Item tool, out float damageReceived, Type damageDealer = null, float experienceMultiplier = 1f)
    {
        damageReceived = amount;

        // if the tree is really young, just outright uproot and destroy it.
        if (this.IsSapling) return this.TryKillSapling(pack, damager, amount, context);
        if (context.Parameters == null)                     return this.TryDamageUnfelledTree(pack, damager, amount, context);
        if (context.Parameters.ContainsKey("stump"))        return this.TryDamageStump(pack, damager, amount, context);
        if (context.Parameters.ContainsKey("branch"))       return this.TryDamageBranch(pack, damager, amount, context);
        if (context.Parameters.ContainsKey("slice"))        return this.TryDamageTrunk(pack, damager, amount, context);
        return pack;
    }

    /// <summary> Perform damaging healthy branches and trunk (if it's a fallen tree) </summary>
    private GameActionPack TryDamageTrunk(GameActionPack pack, INetObject damager, float amount, InteractionContext context)
    {
        // If there are still branches, damage them instead.
        for (var branchID = 0; branchID < this.branches.Length; branchID++)
            this.TryDamageBranch(pack, this.branches[branchID], branchID, amount, context);

        // If the tree is fallen, damage it
        if (this.Fallen)
        {
            pack.AddGameAction(new ChopTree
            {
                Citizen          = context.Player?.User,
                Species          = this.Species.GetType(),
                ChopperUserID    = this.ChopperUserID,
                ActionLocation   = context.HitPosition.HasValue ? context.HitPosition.Value.XYZi() : context.TargetPosition,
                OnGround         = true,
                AccessNeeded     = AccessType.FullAccess,
                ToolUsed         = context.SelectedItem
            });
        }
        pack.AddPostEffect(() => this.TrySliceTrunk(context));
        return pack;
    }

    private GameActionPack TryKillSapling(GameActionPack pack, INetObject damager, float amount, InteractionContext context)
    {
        pack.AddGameAction(this.CreateChopTreeAction(context, true));
        pack.AddPostEffect(() => EcoSim.PlantSim.DestroyPlant(this, DeathType.Harvesting, killer:damager is Player damagerPlayer ? damagerPlayer.User : null));
        return pack;
    }

    private GameActionPack TryDamageUnfelledTree(GameActionPack pack, INetObject damager, float amount, InteractionContext context)
    {
        var user = (damager as Player)?.User;
        if (this.health <= 0) return pack;

        pack.AddGameAction(this.CreateChopTreeAction(context, this.health <= amount));

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
            }

            if (user != null) (context.SelectedItem as ToolItem)?.AddExperience(user, DamageExperienceMultiplier * damageDone, Localizer.Do($"felling {Localizer.A(this.Species.DisplayName).AppendSpaceIfSet()}{this.Species.UILink()}" /*c: should be unisex translations. e.g.: "you earned xp felling {a/an} {Redwood/Oak}."*/));

            this.MarkDirty();
        });
        return pack;
    }

    private GameActionPack TryDamageStump(GameActionPack pack, INetObject damager, float amount, InteractionContext context)
    {
        if (this.Fallen && this.stumpHealth > 0)
        {
            var player = damager as Player;
            if (player != null)
            {
                pack.AddGameAction(new ChopStump()
                {
                    Citizen        = player.User,
                    ActionLocation = this.Position.XYZi(),
                    Destroyed      = this.stumpHealth <= amount,
                    Species        = this.Species.GetType(),
                    ToolUsed       = context.SelectedItem
                });
            }

            pack.AddPostEffect(() =>
            { 
                this.stumpHealth = Mathf.Max(0, this.stumpHealth - amount);

                if (this.stumpHealth <= 0)
                {
                    if (World.GetBlock(this.Position.XYZi()).GetType() == this.Species.BlockType) World.DeleteBlock(this.Position.XYZi());
                    this.stumpHealth = 0;
                    //give tree resources
                    if (player != null)
                    {
                        var changes = InventoryChangeSet.New(player.User.Inventory, player.User);
                        var trunkResources = this.Species.TrunkResources;
                        if (trunkResources != null) trunkResources.ForEach(x => changes.AddItems(x.Key, x.Value.RandInt));
                        else DebugUtils.Fail("Trunk resources missing for: " + this.Species.Name);
                        changes.TryApply();
                    }
                    this.RPC("DestroyStump");

                    // Let another plant grow here
                    EcoSim.PlantSim.UpRootPlant(this);
                }

                this.MarkDirty();
                this.CheckDestroy();
            });
        }

        return pack;
    }

    private GameActionPack TryDamageBranch(GameActionPack pack, INetObject damager, float amount, InteractionContext context)
    {
        int branchID = context.Parameters["branch"];
        var branch   = this.branches[branchID];

        if (context.Parameters.ContainsKey("leaf")) // damage leaf
        {
            int leafID = context.Parameters["leaf"];            
            var leaf   = branch.Leaves[leafID];

            if (leaf.Health > 0)
            {
                pack.AddGameAction(this.CreateChopTreeAction(context, false, true));
                pack.AddPostEffect(() =>
                {
                    if ((leaf.Health = Mathf.Max(0, leaf.Health - amount)) == 0) this.RPC("DestroyLeaves", branchID, leafID);
                    this.MarkDirty();
                });
            }

            return pack;
        }
        else return this.TryDamageBranch(pack, branch, branchID, amount, context);
    }

    private GameActionPack TryDamageBranch(GameActionPack pack, TreeBranch branch, int branchID, float amount, InteractionContext context)
    {
        if (branch != null && branch.Health > 0)
        {
            pack.AddGameAction(this.CreateChopTreeAction(context, false, true));
            pack.AddPostEffect(() =>
            { 
                if ((branch.Health = Mathf.Max(0, branch.Health - amount)) == 0) { this.RPC("DestroyBranch", branchID); this.MarkDirty(); }
            });
        }
        return pack;
    }

    public override void SendInitialState(BSONObject bsonObj, INetObjectViewer viewer)
    {
        base.SendInitialState(bsonObj, viewer);

        // if we have trunk pieces, send those
        var trunkInfo = BSONArray.New;
        if (this.trunkPieces.Count > 0)
        {
            foreach (var trunkPiece in this.trunkPieces)
                trunkInfo.Add(trunkPiece.ToInitialBson());
        }
        bsonObj["trunks"] = trunkInfo;

        if (this.Controller != null && this.Controller is INetObject)
            bsonObj["controller"] = ((INetObject)this.Controller).ID;

        bsonObj["mult"] = this.ResourceMultiplier;
        bsonObj["density"] = this.Species.Density;
    }

    public override void SendUpdate(BSONObject bsonObj, INetObjectViewer viewer)
    {
        base.SendUpdate(bsonObj, viewer);

        if (this.Fallen && this.Controller != viewer)
        {
            var trunkInfo = BSONArray.New;
            foreach (var trunkPiece in this.trunkPieces)
            {
                if (trunkPiece.Position == Vector3.Zero)
                    continue;
                if (trunkPiece.LastUpdateTime < viewer.LastSentUpdateTime)
                    continue;

                trunkInfo.Add(trunkPiece.ToUpdateBson());
            }
            bsonObj["trunks"] = trunkInfo;
            bsonObj["time"] = this.lastKeyframeTime;
        }
    }

    public override void ReceiveUpdate(BSONObject bsonObj)
    {
        bool changed = false;
        if (!bsonObj.ContainsKey("trunks"))
            return;
        BSONArray trunks = bsonObj["trunks"].ArrayValue;
        foreach (BSONObject obj in trunks)
        {
            Guid id = obj["id"];
            TrunkPiece piece = this.trunkPieces.FirstOrDefault(p => p.ID == id);

            if (piece != null && (piece.Position != obj["pos"] || piece.Rotation != obj["rot"]))
            {
                piece.Position = obj["pos"];
                piece.Rotation = obj["rot"];
                piece.Velocity = obj["v"];
                piece.LastUpdateTime = TimeUtil.Seconds;
                changed = true;
            }
        }

        if (changed)
        {
            this.lastKeyframeTime = bsonObj["time"];
            this.LastUpdateTime = TimeUtil.Seconds;
        }
    }

    #region mostly copied from NetPhysicsEntity
    public override bool IsRelevant(INetObjectViewer viewer)
    {
        if (viewer is IWorldObserver observer)
        {
            var closestWrapped = World.ClosestWrappedLocation(observer.Position, this.Position);
            var visibleDistance = observer.ViewDistance.Visible;
            var v = closestWrapped - observer.Position;
            if (Mathf.Abs(v.X) < visibleDistance && Mathf.Abs(v.Z) < visibleDistance)
            {
                if (this.Controller == null)
                    this.SetPhysicsController(observer);
                return true;
            }
        }
        return false;
    }

    public override bool IsNotRelevant(INetObjectViewer viewer)
    {
        if (viewer is not IWorldObserver observer) return false;                                   // only can check for IWorldObserver
        var closestWrapped     = World.ClosestWrappedLocation(observer.Position, this.Position);
        var notVisibleDistance = observer.ViewDistance.NotVisible;
        var v                  = closestWrapped - observer.Position;
        if (Mathf.Abs(v.X) >= notVisibleDistance || Mathf.Abs(v.Z) >= notVisibleDistance)         // check if any of horizontal distances to viewer length enough to go out of view
        {
            if (this.Controller != null && this.Controller.Equals(viewer))                        // in that case reset owning physics controller
                this.SetPhysicsController(null);
            return true;                                                                          // and return true
        }
        else
        {
            // Still relevant, Check if viewer would be a better controller (observer is very close and current controller is far enough)
            if (this.Controller is not IWorldObserver current)
                this.SetPhysicsController(observer);
            else if (current != observer && Vector2.WrappedDistance(observer.Position.XZ(), this.Position.XZ()) < 10f)
            {
                if (Vector2.WrappedDistance(current.Position.XZ(), this.Position.XZ()) > 15f)
                    this.SetPhysicsController(observer);
            }
            return false;
        }
    }

    public bool SetPhysicsController(INetObjectViewer owner)
    {
        // Trees don't need physics until felled
        if (!this.Fallen)
            return false;

        if (Equals(this.Controller, owner))
            return false;

        this.Controller?.RemoveDestroyAction(this.RemovePhysicsController);

        this.Controller = owner;

        this.Controller?.AddDestroyAction(this.RemovePhysicsController);

        if (owner is INetObject netObject)
            this.NetObj.Controller.RPC("UpdateController", netObject.ID);

        return true;
    }

    void RemovePhysicsController()
    {
        this.SetPhysicsController(null);
    }
    #endregion

    public override bool IsUpdated(INetObjectViewer viewer)
    {
        if (this.LastUpdateTime > viewer.LastSentUpdateTime) return true;
        return false;
    }

    public override void Destroy()
    {
        var treeBlockCheck = this.Position.XYZi();
        for (int i = 0; i <= GrowthThresholds.Length; i++)
        {
            var block = World.GetBlock(treeBlockCheck);
            if (block.GetType() == this.Species.BlockType)
                World.DeleteBlock(treeBlockCheck);
            if (block.Is<Solid>())
                break;
            treeBlockCheck += Vector3i.Up;
        }
        MinimapManager.Obj.Objects.Remove(this.minimapObject);
        base.Destroy();
    }

    bool IInteractionChecker.CanInteract(Vector3 objectPosition, Vector3 interactPosition, InteractionInfo info) => this.CanInteract(objectPosition, interactPosition, info);

    protected virtual bool CanInteract(Vector3 objectPosition, Vector3 interactPosition, InteractionInfo info)
    {
        TrunkPiece trunk = null;
        if (info.Parameters != null)
        {
            if (info.Parameters.TryGetGuidValue("id", out var id))
                trunk = this.trunkPieces.FirstOrDefault(x => x.ID == id);
            else if (info.Parameters.TryGetFloatValue("slice", out var slicePoint))
                trunk = this.GetTrunkPiece(slicePoint);
        }
        if (trunk != null) // disable interaction check for trunks, because they may be very tall and we don't have information about max size on the server yet
            return true;
        return IInteractableObject.DefaultCanInteract(objectPosition, interactPosition, info);
    }

    public override void Tick()
    {
        // force update when moving out of sapling
        if (this.GrowthPercent >= this.nextGrowthUpdateThreshold)
        {
            this.LastUpdateTime = TimeUtil.Seconds;
            this.lastGrowthPercent = this.GrowthPercent;
            this.nextGrowthUpdateThreshold = this.GrowthPercent < this.SaplingGrowthPercent ? this.SaplingGrowthPercent : // update when passes sapling growth
                this.GrowthPercent < 1.0f ? Math.Min(this.GrowthPercent + AgeUpdateDifference, 1.0f) : // update every AgeUpdateDifference and when gets to 1.0
                100.0f; // no more updates when at 1.0f, set next update to something unobtainable
        }
    }
}
