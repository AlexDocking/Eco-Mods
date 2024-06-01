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
using Eco.Shared.Utils;
using System;
using System.Linq;

namespace XPBenefits.Tests
{
    using Eco.Core.Controller;
    using Eco.Core.Plugins.Interfaces;
    using Eco.Core.Systems;
    using Eco.Core.Utils;
    using Eco.Gameplay.EcopediaRoot;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using EcoTestTools;
    using System.Collections.Generic;

    [ChatCommandHandler]
    public static class Tests
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestFoodXPCalculation()
        {
            Test.Run(ShouldNotLimitFractionFoodXP);
            Test.Run(ShouldLimitFractionFoodXP);
            Test.Run(ShouldNotLimitFractionFoodXPUser);
            Test.Run(ShouldLimitFractionFoodXPUser);
        }
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestHousingXPCalculation()
        {
            Test.Run(ShouldNotLimitFractionHousingXP);
            Test.Run(ShouldLimitFractionHousingXP);
        }
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestXPBenefitPlugin()
        {
            Test.Run(ShouldCreateEcopediaPagesForBenefits);
        }
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestBenefitFunctions()
        {
            Test.Run(ShouldCalculateFoodBenefitFunction);
            Test.Run(ShouldCalculateHousingBenefitFunction);
            Test.Run(ShouldCalculateGeometricFoodHousingBenefitFunction);
            Test.Run(ShouldCalculateSkillRateBenefitFunction);
        }
        public static void ShouldNotLimitFractionFoodXP()
        {
            XPConfig config = new XPConfig();
            DifficultySettings.SkillGainMultiplier = 1;
            config.DefaultBaseFoodXP = 5;
            config.DefaultMaximumFoodXP = 10;
            Assert.AreEqual(1.2f, SkillRateUtil.FractionFoodXP(11, config, false));
        }
        public static void ShouldLimitFractionFoodXP()
        {
            XPConfig config = new XPConfig();

            DifficultySettings.SkillGainMultiplier = 1;
            config.DefaultBaseFoodXP = 5;
            config.DefaultMaximumFoodXP = 10;
            Assert.AreEqual(1f, SkillRateUtil.FractionFoodXP(11, config, true));
        }
        public static void ShouldNotLimitFractionFoodXPUser()
        {
            DifficultySettings.SkillGainMultiplier = 1;
            User testUser = UserManager.Users.First();

            SetFood(testUser);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.25f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 0.5f;

            Assert.AreEqual(3f, SkillRateUtil.FractionFoodXP(testUser, xpConfig, false));
        }
        public static void ShouldLimitFractionFoodXPUser()
        {
            DifficultySettings.SkillGainMultiplier = 1;
            User testUser = UserManager.Users.First();

            SetFood(testUser);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.25f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 0.5f;

            Assert.AreEqual(1f, SkillRateUtil.FractionFoodXP(testUser, xpConfig, true));
        }
        public static void ShouldNotLimitFractionHousingXP()
        {
            XPConfig config = new XPConfig();

            config.DefaultMaximumHousingXP = 10;
            Assert.AreEqual(1.1f, SkillRateUtil.FractionHousingXP(11, config, false));
        }
        public static void ShouldLimitFractionHousingXP()
        {
            XPConfig config = new XPConfig();

            config.DefaultMaximumHousingXP = 10;
            Assert.AreEqual(1f, SkillRateUtil.FractionHousingXP(11, config, true));
        }
        public static void ShouldCreateEcopediaPagesForBenefits()
        {
            ExampleBenefit benefitWithEcopedia = XPBenefitsPlugin.Obj.Benefits.OfType<ExampleBenefit>().SingleOrDefault();
            BenefitEcopediaGenerator ecopediaGenerator = XPBenefitsEcopediaManager.Obj.GetEcopedia(benefitWithEcopedia) as BenefitEcopediaGenerator;
            Assert.IsNotNull(ecopediaGenerator);
            EcopediaPage ecopediaPage = ecopediaGenerator.GetPage();
            Assert.IsNotNull(ecopediaPage);
            Assert.AreEqual(ecopediaGenerator.PagePriority, ecopediaPage.Priority);
            Assert.IsTrue(Ecopedia.Obj.Categories["XP Benefits"].Pages.ContainsValue(ecopediaPage));
        }
        public static void ShouldCalculateFoodBenefitFunction()
        {
            User testUser = UserManager.Users.First();

            SetFood(testUser);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.5f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 2;

            IBenefitFunction foodBenefitFunction = new FoodBenefitFunction(xpConfig, 10, false);
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

            SetFood(testUser);
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

            SetFood(testUser);
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
        private static void SetFood(User user)
        {
            user.Stomach.Contents.Clear();

            var food = new StomachEntry();
            food.Food = Item.Get<WildStewItem>();
            food.TimeEaten = TimeUtil.Days;
            user.Stomach.Contents.Add(food);
        }
    }
}
