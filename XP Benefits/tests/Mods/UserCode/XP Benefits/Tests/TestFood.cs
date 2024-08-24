using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Serialization;

namespace XPBenefits.Tests
{
    /// <summary>
    /// Remove dependency on SLG foods which could change in future updates and break tests which rely on specific food xp values from nutrients and calories
    /// </summary>
    [Serialized]
    public class TestFood : FoodItem
    {
        public override Nutrients Nutrition => new Nutrients() { Carbs = 10, Fat = 10, Protein = 10, Vitamins = 10 };

        public override float Calories => 1000;

        protected override float BaseShelfLife => 0;
    }
}
