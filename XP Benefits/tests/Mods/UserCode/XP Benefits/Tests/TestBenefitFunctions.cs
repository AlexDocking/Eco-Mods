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
            xpConfig.DefaultBaseFoodXP = foodXP * 0.5f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 2;

            IBenefitFunction foodBenefitFunction = new FoodBenefitFunctionFactory().Create(xpConfig, 10, false);
            Assert.AreEqual(10 / 3f, foodBenefitFunction.CalculateBenefit(testUser));
        }

        public static void ShouldCalculateHousingBenefitFunction()
        {
            User testUser = UserManager.Users.MaxBy(user => SkillRateUtil.HousingXP(user));

            float housingXP = SkillRateUtil.HousingXP(testUser);
            Assert.AreNotEqual(0, housingXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultMaximumHousingXP = housingXP;

            IBenefitFunction housingBenefitFunction = new HousingBenefitFunction(xpConfig, 10, false);
            Assert.AreEqual(10, housingBenefitFunction.CalculateBenefit(testUser));

            xpConfig.DefaultMaximumHousingXP = housingXP * 2;

            Assert.AreEqual(10 * 0.5f, housingBenefitFunction.CalculateBenefit(testUser));
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
            xpConfig.DefaultMaximumHousingXP = housingXP * 2;

            //Fraction of food xp is 1/3
            //Fraction of housing xp is 1/2
            //Geometric mean is 1/sqrt(6)
            IBenefitFunction geometricMeanFoodHousingBenefitFunction = new GeometricMeanFoodHousingBenefitFunction(xpConfig, 10, false);
            Assert.AreEqual(10 * (float)(1 / Math.Sqrt(6)), geometricMeanFoodHousingBenefitFunction.CalculateBenefit(testUser));
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

            //Maximum xp is 1.5 * food xp + 0.5 * housing xp
            //Actual xp is (1 - 0.5) * food xp + housing xp
            //Fraction therefore is (1/3 * food + housing) / (1.5 * food + 0.5 * housing)
            IBenefitFunction skillRateBenefitFunction = new SkillRateBenefitFunction(xpConfig, 10, false);
            Assert.AreEqual(10 * (0.5f * foodXP + housingXP) / (1.5f * foodXP + 0.5f * housingXP), skillRateBenefitFunction.CalculateBenefit(testUser));
        }
    }
}
