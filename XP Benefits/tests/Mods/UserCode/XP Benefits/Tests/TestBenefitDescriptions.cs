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
using EcoTestTools;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestBenefitDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestBenefitFunctionDescriptions()
        {
            Test.Run(ShouldDescribeFoodBenefitFunction);
        }

        public static void ShouldDescribeFoodBenefitFunction()
        {
            User user = TestUtils.TestUser;
            user.ResetStomach(TestingUtils.SingleFood);
            XPConfig config = new XPConfig();
            //User has 1/3 of the maximum food xp
            config.DefaultMaximumFoodXP = SkillRateUtil.FoodXP(user) * 3;
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new FoodBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitDescriber benefitDescriber = benefitFunction.Describer;

            Assert.AreEqual("<style=\"Positive\">2.44</style>", (string)benefitDescriber.CurrentBenefit(user));
            Assert.AreEqual("<color=#FF7C00FF>34</color> food XP", (string)benefitDescriber.CurrentInput(user));
            Assert.AreEqual("<color=#FF7C00FF>2.4</color>", (string)benefitDescriber.CurrentBenefitEcopedia(user));
            Assert.AreEqual("<link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier", (string)benefitDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">10</style>", (string)benefitDescriber.MaximumBenefit(user));
            Assert.AreEqual("<style=\"Positive\">102</style> food XP", (string)benefitDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitDescriber.MeansOfImprovingStat(user));

            //Generate the above asserts
            /*LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitDescriber).GetMethods())
            {
                locStringBuilder.AppendLine(Localizer.Do($"Assert.AreEqual(\"{method.Invoke(benefitDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
            */
        }
    }
}