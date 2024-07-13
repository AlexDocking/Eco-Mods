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
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
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
            Test.Run(ShouldCalculateFoodBenefitFunction);
            Test.Run(ShouldCalculateHousingBenefitFunction);
            Test.Run(ShouldCalculateGeometricFoodHousingBenefitFunction);
            Test.Run(ShouldCalculateSkillRateBenefitFunction);
        }
        public static void ShouldCalculateFoodBenefitFunction()
        {
            User testUser = UserManager.Users.First();

            testUser.ResetStomach(TestingUtils.SingleFood);
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

            testUser.ResetStomach(TestingUtils.SingleFood);
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
            Assert.AreEqual(10 * (float)Math.Sqrt(2/3f), geometricMeanFoodHousingBenefitFunction.CalculateBenefit(testUser));

            //After clamp:
            //Fraction of food xp is 1/3
            //Fraction of housing xp is 1
            //Geometric mean is sqrt(1/3f)
            geometricMeanFoodHousingBenefitFunction = new GeometricMeanFoodHousingBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10 * (float)Math.Sqrt(1/3f), geometricMeanFoodHousingBenefitFunction.CalculateBenefit(testUser));
        }
        public static void ShouldCalculateSkillRateBenefitFunction()
        {
            User testUser = UserManager.Users.MaxBy(user => SkillRateUtil.HousingXP(user));

            float housingXP = SkillRateUtil.HousingXP(testUser);
            Assert.AreNotEqual(0, housingXP);

            testUser.ResetStomach(TestingUtils.SingleFood);
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
            Assert.AreEqual(10 * (0.5f * (1/3f + 2)), skillRateBenefitFunction.CalculateBenefit(testUser));

            //Clamp % housing score to 1
            skillRateBenefitFunction = new SkillRateBenefitFunctionFactory().Create(xpConfig, 10, true);
            Assert.AreEqual(10 * (0.5f * (1/3f + 1)), skillRateBenefitFunction.CalculateBenefit(testUser));
        }
    }
}
