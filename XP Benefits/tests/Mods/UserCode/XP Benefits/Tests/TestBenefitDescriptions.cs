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
            Test.Run(ShouldDescribeExtraCarryStackLimitBenefit);
        }

        private static void ShouldDescribeHousingXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            user.BuildHouse();
            XPConfig config = new XPConfig();
            //User has 1/4 of the maximum housing xp
            config.DefaultMaximumHousingXP = SkillRateUtil.HousingXP(user) * 4;
            BenefitValue maximumBenefit = new BenefitValue(5);
            IBenefitFunction benefitFunction = new HousingBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitDescriber benefitDescriber = benefitFunction.Describer;


            Assert.AreEqual("<style=\"Positive\">1.25</style>", (string)benefitDescriber.CurrentBenefit(user));
            Assert.AreEqual("<style=\"Positive\">3</style> housing XP", (string)benefitDescriber.CurrentInput(user));
            Assert.AreEqual("<color=#FF7F00FF>1.25</color>", (string)benefitDescriber.CurrentBenefitEcopedia(user));
            Assert.AreEqual("<link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multiplier", (string)benefitDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">5</style>", (string)benefitDescriber.MaximumBenefit(user));
            Assert.AreEqual("<style=\"Positive\">120</style> housing XP", (string)benefitDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <link=\"UnserializedEntry:71\"><style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style></link> multiplier", (string)benefitDescriber.MeansOfImprovingStat(user));
            
            //Generate the above asserts
            /*
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitDescriber).GetMethods())
            {
                locStringBuilder.AppendLine(Localizer.Do($"Assert.AreEqual(\"{method.Invoke(benefitDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
            */
        }

        public static void ShouldDescribeFoodXPBenefitFunction()
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
        public static void ShouldDescribeExtraCarryStackLimitBenefit()
        {
            User user = TestUtils.TestUser;
            user.ResetStomach(TestingUtils.SingleFood);
            XPConfig config = new XPConfig();
            //User has 1/3 of the maximum food xp
            config.DefaultMaximumFoodXP = SkillRateUtil.FoodXP(user) * 3;
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
            
            //Generate the above asserts
            /*
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitDescriber).GetMethods())
            {
                locStringBuilder.AppendLine(Localizer.Do($"Assert.AreEqual(\"{method.Invoke(benefitDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
            */
        }
    }
}