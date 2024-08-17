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
using Eco.Gameplay.Systems.Balance;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using EcoTestTools;
using System.Linq;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestSkillRateUtil
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestFoodXPCalculation()
        {
            Test.Run(ShouldNotLimitFractionFoodXP);
            Test.Run(ShouldLimitFractionFoodXP);
            Test.Run(ShouldNotLimitFractionFoodXPUser);
            Test.Run(ShouldLimitFractionFoodXPUser);
        }
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestHousingXPCalculation()
        {
            Test.Run(ShouldNotLimitFractionHousingXP);
            Test.Run(ShouldLimitFractionHousingXP);
        }
        public static void ShouldNotLimitFractionFoodXP()
        {
            XPConfig config = new XPConfig();
            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;
            config.DefaultBaseFoodXP = 5;
            config.DefaultMaximumFoodXP = 10;
            Assert.AreEqual(1.2f, SkillRateUtil.FractionFoodXP(11, config, false));
        }
        public static void ShouldLimitFractionFoodXP()
        {
            XPConfig config = new XPConfig();

            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;
            config.DefaultBaseFoodXP = 5;
            config.DefaultMaximumFoodXP = 10;
            Assert.AreEqual(1f, SkillRateUtil.FractionFoodXP(11, config, true));
        }
        public static void ShouldNotLimitFractionFoodXPUser()
        {
            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;
            User testUser = UserManager.Users.First();

            testUser.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            float foodXP = SkillRateUtil.FoodXP(testUser);
            Assert.AreNotEqual(0, foodXP);

            XPConfig xpConfig = new XPConfig();
            xpConfig.DefaultBaseFoodXP = foodXP * 0.25f;
            xpConfig.DefaultMaximumFoodXP = foodXP * 0.5f;

            Assert.AreEqual(3f, SkillRateUtil.FractionFoodXP(testUser, xpConfig, false));
        }
        public static void ShouldLimitFractionFoodXPUser()
        {
            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;
            User testUser = UserManager.Users.First();

            testUser.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
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
    }
}