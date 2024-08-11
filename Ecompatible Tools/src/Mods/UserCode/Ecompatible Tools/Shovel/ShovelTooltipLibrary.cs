using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Localization;
using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System.Linq;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;

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
                FloatValue = shovel.MaxTake,
                Shovel = shovel
            };
            float shovelLimit = ValueResolvers.Tools.Shovel.MaxTakeResolver.Resolve(modification, out AuxillaryInfo auxillaryInfo);
            Log.WriteLine(Localizer.Do($"Tooltip shovel limit:{shovelLimit},maxtake {shovel.MaxTake}"));
            if (shovelLimit <= 0) return default;
            LocString modificationDescription = DescriptionGenerator.Obj.BuildModificationListDescriptionInt(auxillaryInfo);
            if (modification.TargetItem != null && modification.TargetItem.MarkedUpName.IsSet())
                return new TooltipSection(Localizer.Do($"Shovel can dig {TextLoc.InfoLight(TextLoc.Foldout(Localizer.NotLocalizedStr(Text.Num(shovelLimit)), Localizer.DoStr("Shovel Dig Limit"), modificationDescription))} {modification.TargetItem?.MarkedUpName} blocks."));
            return new TooltipSection(Localizer.Do($"Shovel can dig {TextLoc.InfoLight(TextLoc.Foldout(Localizer.NotLocalizedStr(Text.Num(shovelLimit)), Localizer.DoStr("Shovel Dig Limit"), modificationDescription))} blocks."));

        }
        
    }
}
