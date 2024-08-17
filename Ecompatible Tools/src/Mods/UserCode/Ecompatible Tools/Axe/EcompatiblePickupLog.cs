using Eco.Gameplay.GameActions;
using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.SharedTypes;
using Ecompatible;
using System;
using System.Linq;
using Vector3 = System.Numerics.Vector3;
using Eco.Shared.Math;
using Eco.Shared.Networking;
using Eco.Shared.Utils;
namespace Eco.Mods.Organisms
{
    public partial class TreeEntity
    {
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
                                    (ContextProperties.Axe, player.User.Inventory.Toolbar.SelectedItem as AxeItem),
                                    (ContextProperties.Tree, this)
                                    );
                                int maxStackSize = ValueResolvers.Tools.Axe.MaxPickupLogsResolver.ResolveInt(0, context);
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
    }
}
