using Eco.Gameplay.Housing.PropertyValues;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Property;
using Eco.Mods.TechTree;
using Eco.Shared.Voxel;
using System;

namespace XPBenefits.Tests
{
    public static class TestingUtils
    {
        public static Type[] SingleFood => new Type[] { typeof(TestFood) };

        public static void CreateTestResidencyWithValue(this User user, float propertyValue)
        {
            Deed deed = DeedFactory.CreateDeed(user);
            //Claim the plot at (0, 0) so that the deed can have a housing score
            PropertyManager.ForceClaim(deed, user, new PlotPos(), true);
            deed.Residency.DebugForceResidency(user);
            deed.PropertyValueBoost = propertyValue;
            PropertyValueManager.Obj.UpdateProperty(deed);
            deed.Save();
        }

        public static void MakeHomeless(this User user) => user.GetResidencyHouse()?.Residency?.DebugForceEvict(user);

        public static void ReplaceStomachContentsAndMakeTasteOk(this User user, params Type[] foodTypes)
        {
            user.Stomach.Contents.Clear();

            foreach (Type foodType in foodTypes)
            {
                if (Item.Get(foodType) is not FoodItem foodItem) continue;
                user.Stomach.TasteBuds.FoodToTaste[foodType] = new ItemTaste() { Discovered = true, Preference = ItemTaste.TastePreference.Ok };
                user.Stomach.Eat(foodItem, out _, force: true);
            }
        }
    }
}