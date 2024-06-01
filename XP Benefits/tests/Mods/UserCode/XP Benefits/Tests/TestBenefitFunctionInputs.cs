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
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using EcoTestTools;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestBenefitFunctionInputs
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestBenefitFunctionInputDescriptions()
        {
            Test.Run(ShouldDescribeFoodXP);
        }
        public static void ShouldDescribeFoodXP()
        {
            User user = TestUtils.TestUser;
            user.ResetStomach(TestingUtils.SingleFood);
            XPConfig config = new XPConfig();
            //User has 1/3 of the maximum food xp
            config.DefaultMaximumFoodXP = SkillRateUtil.FoodXP(user) * 3;
            FoodXPInput input = new FoodXPInput(config);
            InputDescriber inputDescriber = new InputDescriber(input, "food XP");
            string currentInputDescription = inputDescriber.DescribeInput(user);
            string expected = Localizer.Do($"{DisplayUtils.GradientNumLoc(SkillRateUtil.FoodXP(user), SkillRateUtil.FoodXP(user).ToString("0.#"), new Eco.Shared.Math.Range(config.BaseFoodXP, config.MaximumFoodXP))} food XP");
            Assert.AreEqual(expected, currentInputDescription);
        }
    }
}
