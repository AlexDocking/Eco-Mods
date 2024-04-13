// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

using Eco.Mods.TechTree;

namespace CompatibleTools
{
    using Eco.Shared.Items;
    using Eco.Shared.Localization;
    using Eco.Gameplay.Players;
    using Eco.Shared.Utils;
    using System.Linq;
    using CompatibleTools;
    using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
    using Eco.Gameplay.Systems.NewTooltip;
    using Eco.Gameplay.Items;
    using Eco.Core.Utils;

    [TooltipLibrary]
    public static class ShovelTooltipLibrary
    {
        [NewTooltip(CacheAs.Disabled, 15)]
        public static LocString ShovelMaxTakeTooltip(this ShovelItem shovel, User user)
        {
            ShovelMaxTakeModification modification = new ShovelMaxTakeModification()
            {
                User = user,
                TargetItem = user.Inventory.Carried.Stacks.First().Item,
                InitialMaxTake = shovel.MaxTake,
                MaxTake = shovel.MaxTake,
                Shovel = shovel
            };
            int? shovelLimit = shovel.CalculateMaxTake(modification, ShovelItem.MaxTakeModifiers);
            if (shovelLimit.HasValue)
            {
                return new TooltipSection(Localizer.Do($"Shovel can dig {shovelLimit.Value} {modification.TargetItem?.MarkedUpName.AppendSpaceIfSet()}blocks."));
            }

            return LocString.Empty;
        }
    }
}
