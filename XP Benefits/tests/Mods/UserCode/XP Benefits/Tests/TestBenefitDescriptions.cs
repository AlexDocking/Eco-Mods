using Eco.Core.Tests;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.Balance;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Gameplay.Utils;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using Eco.Shared.Utils;
using EcoTestTools;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public class TestBenefitDescriptions
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
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
            user.ReplaceStomachContentsAndMakeTasteOk(TestingUtils.SingleFood);
            user.CreateTestResidencyWithValue(3);

            if (SkillRateUtil.FoodXP(user) <= 0) throw new Exception("Could not give food xp");
            if (SkillRateUtil.HousingXP(user) <= 0) throw new Exception("Could not give housing xp");

            BalancePlugin.Obj.Config.SkillGainMultiplier = 1;

            XPConfig config = new XPConfig();
            //User has 1/3 of the maximum food xp
            config.DefaultMaximumFoodXP = config.DefaultBaseFoodXP + (SkillRateUtil.FoodXP(user) / BalancePlugin.Obj.Config.SkillGainMultiplier - config.DefaultBaseFoodXP) * 3f;
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
        private static void PrintAssertsForIBenefitDescriberMethods(IBenefitDescriber benefitDescriber, User user)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            foreach (MethodInfo method in typeof(IBenefitDescriber).GetInterfaces().SelectMany(@interface => @interface.GetMethods()).OrderBy(m => m.Name))
            {
                locStringBuilder.AppendLine(Localizer.NotLocalized($"Assert.AreEqual(\"{method.Invoke(benefitDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitDescriber)}.{method.Name}({nameof(user)}));"));
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
            foreach (MethodInfo method in typeof(IBenefitInputDescriber).GetInterfaces().SelectMany(@interface => @interface.GetMethods()).OrderBy(m => m.Name))
            {
                locStringBuilder.AppendLine(Localizer.NotLocalized($"Assert.AreEqual(\"{method.Invoke(benefitInputDescriber, new object[] { user }).ToString().Replace("\"", "\\\"")}\", (string){nameof(benefitInputDescriber)}.{method.Name}({nameof(user)}));"));
            }
            Log.WriteLine(locStringBuilder.ToLocString());
        }
        /// <summary>
        /// Remove most of the UnserializedEntry tags so they don't break tests when the number changes between Eco builds. The important part is the icon name and that will still be compared
        /// </summary>
        /// <param name="messageString"></param>
        /// <returns></returns>
        private static object CleanIrrelevantTagDetails(object messageString)
        {
            if (messageString is not string message) return null;
            StringBuilder cleanedMessageBuilder = new StringBuilder();
            var split = message.Split("UnserializedEntry:");
            foreach (var part in split)
            {
                cleanedMessageBuilder.Append(String.Concat(part.SkipWhile(c => char.IsDigit(c))));
            }
            return cleanedMessageBuilder.ToString();
        }
        private static void ShouldDescribeFoodXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new FoodBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;

            Assert.AreEqual("<color=#FFAA00FF>72</color> food XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "<link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">192</style> food XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "You can increase this benefit by improving your <link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        private static void ShouldDescribeHousingXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new HousingBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;
            
            Assert.AreEqual("<color=#FF6600FF>3</color> housing XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style> multiplier", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">15</style> housing XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style> multiplier", (string)benefitInputDescriber.MeansOfImprovingStat(user));

        }

        private static void ShouldDescribeGeometricMeanFoodHousingXPBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new GeometricMeanFoodHousingBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;
            
            Assert.AreEqual("<color=#FFAA00FF>33%</color> food XP and <color=#FF6600FF>20%</color> housing XP", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "<link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> and <style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style> multipliers", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">192</style> food XP and <style=\"Positive\">15</style> housing XP", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "You can increase this benefit by improving your <link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> and <style=\"Item\"><icon name=\"House\" type=\"\">Housing</icon></style> multipliers. If you want to see the greatest improvement you should improve the lowest percentage first. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        private static void ShouldDescribeSkillRateBenefitFunction()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            BenefitValue maximumBenefit = new BenefitValue(10);
            IBenefitFunction benefitFunction = new SkillRateBenefitFunctionFactory().Create(config, maximumBenefit, false);
            IBenefitInputDescriber benefitInputDescriber = benefitFunction.Describer;
            Assert.AreEqual("an XP multiplier of <color=#FFA400FF>75</color>", (string)benefitInputDescriber.CurrentInput(user));
            Assert.AreEqual("<style=\"Item\"><icon name=\"Skill Books\" type=\"\">XP Multiplier</icon></style>", (string)benefitInputDescriber.InputName(user));
            Assert.AreEqual("an XP multiplier of <style=\"Positive\">207</style>", (string)benefitInputDescriber.MaximumInput(user));
            Assert.AreEqual("You can increase this benefit by improving your <style=\"Item\"><icon name=\"Skill Books\" type=\"\">XP Multiplier</icon></style>", (string)benefitInputDescriber.MeansOfImprovingStat(user));
        }

        public static void ShouldDescribeExtraCarryStackLimitBenefit()
        {
            User user = TestUtils.TestUser;
            XPConfig config = CreateConfig(user);
            config.ExtraCarryStackLimitBenefitFunction = new FoodBenefitFunctionFactory().Name;
            config.ExtraCarryStackLimitMaxBenefitValue = 10;
            ExtraCarryStackLimitBenefit benefit = new ExtraCarryStackLimitBenefit();
            benefit.Initialize(true, config, 10, false, new FoodBenefitFunctionFactory().Create(config, 10));
            IBenefitDescriber benefitDescriber = new ExtraCarryStackLimitBenefitDescriber(benefit);

            Assert.AreEqual("<style=\"Positive\">+333%</style>", (string)benefitDescriber.CurrentBenefit(user));
            Assert.AreEqual("<color=#FFAA00FF>+333%</color>", (string)benefitDescriber.CurrentBenefitEcopedia(user));
            Assert.AreEqual("<color=#FFAA00FF>72</color> food XP", (string)benefitDescriber.CurrentInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "<link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier", (string)benefitDescriber.InputName(user));
            Assert.AreEqual("<style=\"Positive\">+1000%</style>", (string)benefitDescriber.MaximumBenefit(user));
            Assert.AreEqual("<style=\"Positive\">192</style> food XP", (string)benefitDescriber.MaximumInput(user));
            Assert.AreEqual(CleanIrrelevantTagDetails, "You can increase this benefit by improving your <link=\"UnserializedEntry:83\"><style=\"Item\"><icon name=\"Beet\" type=\"\">Nutrition</icon></style></link> multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage", (string)benefitDescriber.MeansOfImprovingStat(user));
        }
    }
}