using Eco.Gameplay.Housing.PropertyValues;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Property;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using Eco.Shared.Voxel;
using System;
using System.Threading;

namespace XPBenefits.Tests
{
    public static class TestingUtils
    {
        public static Type[] SingleFood => new Type[] { typeof(TestFood) };
        private static Player testPlayer;
        private static Player TestPlayer { get { testPlayer ??= new Player(TestUtils.TestUser, 0, null); return testPlayer; }  }
        public static User TestUser => TestPlayer.User;
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
            bool loggedIn = user.LoggedIn;
            if (!loggedIn)
            {
                _ = new Player(user, 0, null);
            }
            user.Stomach.Contents.Clear();
            user.Stomach.RecalcAverageNutrients();
            foreach (Type foodType in foodTypes)
            {
                if (Item.Get(foodType) is not FoodItem foodItem) continue;
                user.Stomach.TasteBuds.FoodToTaste[foodType] = new ItemTaste() { Discovered = true, Preference = ItemTaste.TastePreference.Ok };
                user.Stomach.Eat(foodItem, out _, force: true);
            }
            if (!loggedIn) user.Logout();
        }
    }
}