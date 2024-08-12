// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using System.Collections.Generic;
    using System.Linq;
    using Eco.Gameplay.GameActions;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Objects;
    using Eco.Gameplay.Players;
    using Eco.Gameplay.Property;
    using Eco.Shared.Networking;
    using Eco.Shared.Utils;
    using System;
    using Eco.Shared.Math;
    using Eco.Shared.Voxel;
    using Ecompatible;

    public partial class MiningSweepingHandsTalent
    {
        public readonly int PickUpRange = 4;
        public override void RegisterTalent(User user)
        {
            base.RegisterTalent(user);
            user.OnPickupingObject.Add(this.ApplyAction);
        }
        public override void UnRegisterTalent(User user)
        {
            base.UnRegisterTalent(user);
            user.OnPickupingObject.Remove(this.ApplyAction);
        }
        void ApplyAction(User user, INetObject target, INetObject tool, GameActionPack pack)
        {
            // only apply talent when object picked up by hands, not with tool like excavator or skid steer
            if (tool != null) return;
            if (target is RubbleObject rubble)
                this.ApplyTalent(user, rubble, pack);
        }

        private void ApplyTalent(User user, RubbleObject target, GameActionPack pack)
        {
            if (!(target is IRepresentsItem representsItem)) return;

            var itemType = representsItem.RepresentedItemType;

            var carried = user.Inventory.Carried;
            var resource = Item.Get(itemType);
            var maxStackSizeContext = new SweepingHandsMaxTakeModificationContext()
            {
                User = user,
                Resource = resource,
                SweepingHandsTalent = this,
            };
            int maxStackSize = ValueResolvers.Tools.Pickaxe.MaxStackSizeResolver.ResolveInt(0, maxStackSizeContext);
            // max stack size minus currently picking item
            var numToTake = maxStackSize - 1;
            if (numToTake <= 0) return;

            var carrying = user.Carrying;
            if (!carrying.Empty())
            {
                // ReSharper disable once PossibleNullReferenceException because Empty checked
                if (carrying.Item.Type != itemType || carrying.Quantity >= numToTake) return;

                // adjust to currently carrying item count
                numToTake -= carrying.Quantity;
            }

            // Get not breakable rubble around the target one and group them by their plot position.
            var originPlotPos = target.Position.XZi().ToPlotPos();
            var pickUpRangeContext = new SweepingHandsMaxTakeModificationContext()
            {
                User = user,
                Resource = resource,
                SweepingHandsTalent = this,
            };
            var pickUpRange = ValueResolvers.Tools.Pickaxe.MiningSweepingHands.PickUpRangeResolver.Resolve(0, pickUpRangeContext);
            var nearbyRubbleGroups = NetObjectManager.Default.GetObjectsWithin(target.Position, pickUpRange)
                                                     .OfType<RubbleObject>()
                                                     .Where(x => x != target && !x.IsBreakable && x is IRepresentsItem rubbleRepresentsItem && rubbleRepresentsItem.RepresentedItemType == itemType)
                                                     .GroupBy(x => x.Position.XZi().ToPlotPos()).ToList();

            // Exexute PickupRubbles for each rubble in current plot
            var currentPlotData = nearbyRubbleGroups.FirstOrDefault(x => x.Key == originPlotPos);
            if (currentPlotData != null) this.CollectRubblesOnPlot(currentPlotData.ToList(), user, pack, itemType, numToTake);

            //todo implement CollectRubblesOnPlot execution after Auth refactor to be able to check auth before adding actions to pack
        }

        // Executes PickupRubbles action on rubble list without notifying (to omit loops for talent reuse) and returns success count
        private int CollectRubblesOnPlot(List<RubbleObject> rubbles, User user, GameActionPack pack, Type itemType, int numToTake)
        {
            var numTaken = 0;

            // Process rubble within the same plot (it will have equal auth requirements and can be done inside the original pack).
            foreach (var rubble in rubbles)
            {
                // Try to add rubble-related stuff to the pack and increment the counter if succeeded.
                if (pack.PickupRubbles(user.Player, user.Inventory, rubble.SingleItemAsEnumerable(), itemType, notificate: false)) numTaken++;

                // No need to continue if it already has required amount.
                if (numTaken == numToTake) break;
            }

            return numTaken;
        }
    }
}
