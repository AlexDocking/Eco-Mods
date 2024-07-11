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
using System;
using System.Reflection;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestBenefitDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
        [CITest]
        public static void TestBenefitFunctionDescriptions()
        {
            Test.Run(ShouldDescribeFoodXPBenefitFunction);
            Test.Run(ShouldDescribeHousingXPBenefitFunction);
            Test.Run(ShouldDescribeGeometricMeanFoodHousingXPBenefitFunction);
            Test.Run(ShouldDescribeSkillRateBenefitFunction);
            Test.Run(ShouldDescribeExtraCarryStackLimitBenefit);
        }

        private static XPConfig CreateConfig(User user)
        {
            user.ResetStomach(TestingUtils.SingleFood);
            user.BuildHouse();
            XPConfig config = new XPConfig();
            //User has 1/3 of the maximum food xp
            config.DefaultMaximumFoodXP = SkillRateUtil.FoodXP(user) * 3f;
            //User has 1/5 of the maximum housing xp
            config.DefaultMaximumHousingXP = SkillRateUtil.HousingXP(user) * 5;
            return config;
        }

        /// <summary>
        /// Procedure to help creating regression tests.
        /// Generate and print to the console the asserts for all the methods in IBenefitDescriber.
        /// </summary>
        /// <param name="benefitDescriber"></param>
        /// <param name="user"></param>
        private static void PrintAssertsForIBenefitDescriberMethods(IBenefitInputDescriber benefitDescriber, User user)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitInputDescriber).GetMethods())
            {
                locStringBuilder.AppendLine(Localizer.Do($"Assert.AreEqual(\"{method.Invoke(benefitDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
        }

        /// <summary>
        /// Procedure to help creating regression tests.
        /// Generate and print to the console the asserts for all the methods in IBenefitInputDescriber.
        /// </summary>
        /// <param name="benefitInputDescriber"></param>
        /// <param name="user"></param>
        private static void PrintAssertsForIBenefitInputDescriberMethods(IBenefitInputDescriber benefitInputDescriber, User user)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitInputDescriber).GetMethods())
            {
                locStringBuilder.AppendLine(Localizer.Do($"Assert.AreEqual(\"{method.Invoke(benefitInputDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitInputDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
        }

        private static void ShouldDescribeFoodXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new FoodBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;

            Assert.AreEqual("<color=#FF7C00FF>34</color> food XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">102</style> food XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        private static void ShouldDescribeHousingXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new HousingBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;

            Assert.AreEqual("<color=#FF6600FF>3</color> housing XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multiplier", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">15</style> housing XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multiplier", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        private static void ShouldDescribeGeometricMeanFoodHousingXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new GeometricMeanFoodHousingBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;

            Assert.AreEqual("<color=#FF7C00FF>24%</color> food XP and <color=#FF6600FF>20%</color> housing XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> and <link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multipliers", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">102</style> food XP and <style=\"Positive\">15</style> housing XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> and <link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multipliers. If you want to see the greatest improvement you should improve the lowest percentage first. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        private static void ShouldDescribeSkillRateBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new SkillRateBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;

            Assert.AreEqual("an XP multiplier of <color=#FF7900FF>37</color>", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<link=\"UnserializedEntry:86\"><style=\"Item\"><icon name=\"Skill Books\" type=\"\">XP Multiplier</icon></style></link>", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("an XP multiplier of <style=\"Positive\">117</style>", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:86\"><style=\"Item\"><icon name=\"Skill Books\" type=\"\">XP Multiplier</icon></style></link>", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        public static void ShouldDescribeExtraCarryStackLimitBenefit()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            config.ExtraCarryStackLimitBenefitFunction = new FoodBenefitFunctionFactory().Name;
            config.ExtraCarryStackLimitMaxBenefitValue = 10;
            ExtraCarryStackLimitBenefit benefit = new ExtraCarryStackLimitBenefit();
            benefit.Initialize(true, 10, false, new FoodBenefitFunctionFactory().Create(config, 10));
            IBenefitDescriber benefitDescriber = new ExtraCarryStackLimitBenefitDescriber(benefit);

            Assert.AreEqual("<style=\"Positive\">+244%</style>", (string)benefitDescriber.CurrentBenefit(user));
            Assert.AreEqual("<color=#FF7C00FF>34</color> food XP", (string)benefitDescriber.CurrentInput(user));
            Assert.AreEqual("<color=#FF7C00FF>+244%</color>", (string)benefitDescriber.CurrentBenefitEcopedia(user));
            Assert.AreEqual("<link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier", (string)benefitDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">+1000%</style>", (string)benefitDescriber.MaximumBenefit(user));
            Assert.AreEqual("<style=\"Positive\">102</style> food XP", (string)benefitDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:87\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitDescriber.MeansOfImprovingStat(user));
        }
    }
}