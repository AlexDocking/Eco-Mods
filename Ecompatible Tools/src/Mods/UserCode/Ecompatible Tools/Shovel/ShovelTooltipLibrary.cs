using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System.Linq;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Items;
using Eco.Shared.Logging;
using System;
using Eco.Gameplay.Systems.Messaging.Notifications;
using Eco.Shared.Networking;
using System.Collections.Generic;
using Eco.Gameplay.Interactions.Interactors;
using System.Collections.Concurrent;

namespace Ecompatible
{
    [TooltipLibrary]
    public static class ShovelTooltipLibrary
    {
        [NewTooltip(CacheAs.Disabled, 15)]
        public static LocString ShovelMaxTakeTooltip(this EcompatibleDig shovel, User user)
        {
            Item targetItem = user.Inventory.Carried.Stacks.First().Item;
            bool targetItemCanBeDug = targetItem != null && shovel.IsDiggable(targetItem.Type);
            IContext modification;

            if (targetItemCanBeDug)
            {
                modification = Context.CreateContext(
                    (ContextProperties.User, user),
                    (ContextProperties.Shovel, shovel),
                    (ContextProperties.TargetItem, targetItem));
            }
            else
            {
                modification = Context.CreateContext(
                    (ContextProperties.User, user),
                    (ContextProperties.Shovel, shovel));
            }

            int shovelLimit = ValueResolvers.Tools.Shovel.MaxTakeResolver.ResolveInt(shovel.MaxTake, modification, out IResolvedSequence<float> resolvedSequence);
            if (shovelLimit <= 0) return default;
            LocString modificationDescription = DescriptionGenerator.Obj.DescribeSequenceAsTableAndRoundDown(resolvedSequence);

            if (targetItemCanBeDug)
            {
                return new TooltipSection(Localizer.Do($"Shovel can dig {TextLoc.InfoLight(TextLoc.Foldout(Localizer.NotLocalizedStr(Text.Num(shovelLimit)), Localizer.DoStr("Shovel Dig Limit"), modificationDescription))} {targetItem.MarkedUpName} blocks."));
            }
            return new TooltipSection(Localizer.Do($"Shovel can dig {TextLoc.InfoLight(TextLoc.Foldout(Localizer.NotLocalizedStr(Text.Num(shovelLimit)), Localizer.DoStr("Shovel Dig Limit"), modificationDescription))} blocks."));
        }
        
    }
}
