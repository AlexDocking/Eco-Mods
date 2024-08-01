using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System.Linq;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Core.Utils;

namespace Ecompatible
{
    [TooltipLibrary]
    public static class ShovelTooltipLibrary
    {
        [NewTooltip(CacheAs.Disabled, 15)]
        public static LocString ShovelMaxTakeTooltip(this ShovelItem shovel, User user)
        {
            ShovelMaxTakeModificationContext modification = new ShovelMaxTakeModificationContext()
            {
                User = user,
                TargetItem = user.Inventory.Carried.Stacks.First().Item,
                IntValue = shovel.MaxTake,
                FloatValue = shovel.MaxTake,
                Shovel = shovel
            };
            int shovelLimit = ValueResolvers.Tools.Shovel.MaxTakeResolver.ResolveInt(modification);
            if (shovelLimit <= 0) return default;
            return new TooltipSection(Localizer.Do($"Shovel can dig {shovelLimit} {modification.TargetItem?.MarkedUpName.AppendSpaceIfSet()}blocks."));
        }
    }
}
