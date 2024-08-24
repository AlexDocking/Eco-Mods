// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System.Linq;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Items;

namespace Ecompatible
{
    [TooltipLibrary]
    public static class ShovelTooltipLibrary
    {
        [NewTooltip(CacheAs.Disabled, 15)]
        public static LocString ShovelMaxTakeTooltip(this ShovelItem shovel, User user)
        {
            Item targetItem = user.Inventory.Carried.Stacks.First().Item;
            bool targetItemCanBeDug = targetItem != null && shovel.IsDiggable(targetItem.Type);
            IShovelPickUpContext modification;

            if (targetItemCanBeDug)
            {
                modification = ContextFactory.CreateShovelPickUpContext(
                    user: user,
                    shovel: shovel,
                    itemToPutInInventory: targetItem
                    );
            }
            else
            {
                modification = ContextFactory.CreateShovelPickUpContext(
                    user: user,
                    shovel: shovel,
                    itemToPutInInventory: targetItem
                    );
            }

            int shovelLimit = ValueResolvers.Tools.Shovel.MaxTakeResolver.ResolveInt(shovel.MaxTake, modification, out var resolvedSequence);
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
