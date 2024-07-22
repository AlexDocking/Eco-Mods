//XP Benefits
//Copyright (C) 2023 Alex Docking
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Utils;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using EcoTestTools;
using System;
using System.Linq;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestBenefitFunctions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestBenefitFunctionTypes()
        {
            Test.Run(SmokeTest, "Smoke test for calorie, weight and stack limit benefits");
            Test.Run(ShouldCalculateFoodBenefitFunction);
            Test.Run(ShouldCalculateHousingBenefitFunction);
            Test.Run(ShouldCalculateGeometricFoodHousingBenefitFunction);
            Test.Run(ShouldCalculateSkillRateBenefitFunction);
            Test.Run(ShouldThrowExceptionInInputConstructorsIfNoConfig);
        }

        public static void ShouldCalculateFoodBenefitFunction()
        {
            User testUser = TestUtils.TestUser;

            testUser.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.25f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 0.75f;

            IBenefitFunction foodBenefitFunction = new FoodBenefitFunctionFactory().Create(xpConfig, 10, false);
            Assert.AreEqual(10 * 1.5f, foodBenefitFunction.CalculateBenefit(testUser));

            foodBenefitFunction = new FoodBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10, foodBenefitFunction.CalculateBenefit(testUser));
        }

        public static void ShouldCalculateHousingBenefitFunction()
        {
            User testUser = UserManager.Users.MaxBy(user => SkillRateUtil.HousingXP(user));

            float housingXP = SkillRateUtil.HousingXP(testUser);
            Assert.AreNotEqual(0, housingXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultMaximumHousingXP = housingXP / 1.5f;

            IBenefitFunction housingBenefitFunction = new HousingBenefitFunctionFactory().Create(xpConfig, 10, false);
            Assert.AreEqual(10 * 1.5f, housingBenefitFunction.CalculateBenefit(testUser));

            housingBenefitFunction = new HousingBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10, housingBenefitFunction.CalculateBenefit(testUser));
        }

        public static void ShouldCalculateGeometricFoodHousingBenefitFunction()
        {
            User testUser = UserManager.Users.MaxBy(user => SkillRateUtil.HousingXP(user));

            float housingXP = SkillRateUtil.HousingXP(testUser);
            Assert.AreNotEqual(0, housingXP);

            testUser.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.5f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 2;
            xpConfig.DefaultMaximumHousingXP = housingXP * 0.5f;

            //Fraction of food xp is 1/3
            //Fraction of housing xp is 2
            //Geometric mean is sqrt(2/3f)
            IBenefitFunction geometricMeanFoodHousingBenefitFunction = new GeometricMeanFoodHousingBenefitFunctionFactory().Create(xpConfig, 10, false);
            Assert.AreEqual(10 * (float)Math.Sqrt(2 / 3f), geometricMeanFoodHousingBenefitFunction.CalculateBenefit(testUser));

            //After clamp:
            //Fraction of food xp is 1/3
            //Fraction of housing xp is 1
            //Geometric mean is sqrt(1/3f)
            geometricMeanFoodHousingBenefitFunction = new GeometricMeanFoodHousingBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10 * (float)Math.Sqrt(1 / 3f), geometricMeanFoodHousingBenefitFunction.CalculateBenefit(testUser));
        }

        public static void ShouldCalculateSkillRateBenefitFunction()
        {
            User testUser = UserManager.Users.MaxBy(user => SkillRateUtil.HousingXP(user));

            float housingXP = SkillRateUtil.HousingXP(testUser);
            Assert.AreNotEqual(0, housingXP);

            testUser.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.5f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 2;
            xpConfig.DefaultMaximumHousingXP = housingXP * 0.5f;

            //Food is 1 in range 0.5-2 -> 1/3 of max
            //Housing is 1 in range 0-0.5 -> 2x max
            //Calculation is based on the average of those
            IBenefitFunction skillRateBenefitFunction = new SkillRateBenefitFunctionFactory().Create(xpConfig, 10, false);
            Assert.AreEqual(10 * (0.5f * (1 / 3f + 2)), skillRateBenefitFunction.CalculateBenefit(testUser));

            //Clamp % housing score to 1
            skillRateBenefitFunction = new SkillRateBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10 * (0.5f * (1 / 3f + 1)), skillRateBenefitFunction.CalculateBenefit(testUser));
        }

        public static void ShouldThrowExceptionInInputConstructorsIfNoConfig()
        {
            Assert.Throws<ArgumentNullException>(() => new FoodXPInput(null));
            Assert.Throws<ArgumentNullException>(() => new HousingXPInput(null));
        }

        /// <summary>
        /// Test that the benefits change the relevant user stats in some way when food and housing is changed.
        /// </summary>
        public static void SmokeTest()
        {
            TestCalorieBenefit(XPBenefitsPlugin.Obj.Benefits.OfType<ExtraCaloriesBenefit>().Single());
            TestCarryStackLimitBenefit(XPBenefitsPlugin.Obj.Benefits.OfType<ExtraCarryStackLimitBenefit>().Single());
            TestCarryWeightLimitBenefit(XPBenefitsPlugin.Obj.Benefits.OfType<ExtraWeightLimitBenefit>().Single());
        }

        private static void TestCalorieBenefit(ExtraCaloriesBenefit benefit) => TestBenefit(benefit, GetCalorieCapacity);

        private static void TestCarryStackLimitBenefit(ExtraCarryStackLimitBenefit benefit) => TestBenefit(benefit, GetCarryStackLimitMultiplier);

        private static void TestCarryWeightLimitBenefit(ExtraWeightLimitBenefit benefit) => TestBenefit(benefit, GetCarryWeightLimit);

        private static void TestBenefit(BenefitBase benefit, Func<User, float> getStat)
        {
            User user = TestUtils.TestUser;
            Assert.IsNotNull(benefit);
            user.ReplaceStomachContentsAndMakeTasteOk();
            user.MakeHomeless();

            benefit.ApplyBenefitToUser(user);

            float initialStat = getStat(user);
            user.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            user.CreateTestResidencyWithValue(3);
            if (SkillRateUtil.FoodXP(user) <= 0) throw new Exception("Could not give food xp");
            if (SkillRateUtil.HousingXP(user) <= 0) throw new Exception("Could not give housing xp");
            float modifiedStat = getStat(user);
            Assert.AreNotEqual(initialStat, modifiedStat);
        }

        private static float GetCalorieCapacity(User user) => user.Stomach.MaxCalories;

        private static float GetCarryStackLimitMultiplier(User user) => user.Inventory.Carried.Restrictions.OfType<StackLimitBenefitInventoryRestriction>().First().BenefitFunction.CalculateBenefit(user);

        private static float GetCarryWeightLimit(User user) => user.Inventory.ToolbarBackpack.WeightComponent.MaxWeight;
    }
}