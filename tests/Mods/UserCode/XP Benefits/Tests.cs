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
    using Eco.Core.Systems;
    using Eco.Gameplay.EcopediaRoot;
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using Eco.Shared.Localization;
    using EcoTests;
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
            Test.Run(ShouldDiscoverBenefits);
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
        public static void ShouldDiscoverBenefits()
        {
            XPBenefitsPlugin plugin = XPBenefitsPlugin.Obj;
            ValidBenefit shouldFindValidBenefit = plugin.Benefits.Where(benefit => benefit.GetType() == typeof(ValidBenefit)).SingleOrDefault() as ValidBenefit;
            Assert.IsNotNull(shouldFindValidBenefit);
            DisabledBenefit shouldNotFindDisabledBenefit = plugin.Benefits.Where(benefit => benefit.GetType() == typeof(DisabledBenefit)).SingleOrDefault() as DisabledBenefit;
            Assert.IsNull(shouldNotFindDisabledBenefit);

            NoDefaultConstructorBenefit shouldNotFindBenefitWithoutDefaultConstructor = plugin.Benefits.Where(benefit => benefit.GetType() == typeof(NoDefaultConstructorBenefit)).SingleOrDefault() as NoDefaultConstructorBenefit;
            Assert.IsNull(shouldNotFindBenefitWithoutDefaultConstructor);

            AbstractBenefit shouldNotFindAbstractBenefit = plugin.Benefits.Where(benefit => benefit.GetType() == typeof(AbstractBenefit)).SingleOrDefault() as AbstractBenefit;
            Assert.IsNull(shouldNotFindAbstractBenefit);
        }
        public static void ShouldCreateEcopediaPagesForBenefits()
        {
            XPBenefitsPlugin plugin = XPBenefitsPlugin.Obj;
            BenefitWithoutEcopediaPage benefitWithoutEcopedia = plugin.Benefits.OfType<BenefitWithoutEcopediaPage>().SingleOrDefault();
            Assert.IsNull(GetEcopediaPage(benefitWithoutEcopedia));

            DisabledBenefit disabledBenefit = new DisabledBenefit();
            Assert.IsNull(GetEcopediaPage(disabledBenefit));

            BenefitWithEcopediaPage benefitWithEcopedia = plugin.Benefits.OfType<BenefitWithEcopediaPage>().SingleOrDefault();
            EcopediaPage ecopediaPage = GetEcopediaPage(benefitWithEcopedia);
            Assert.IsNotNull(ecopediaPage);
            Assert.AreEqual(benefitWithEcopedia.EcopediaPagePriority, ecopediaPage.Priority);
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
        private static EcopediaPage GetEcopediaPage(ILoggedInBenefit benefit) => benefit != null ? Ecopedia.Obj.GetPage(benefit.EcopediaPageName) : null;
    }

    /// <summary>
    /// It is ILoggedInBenefit and doesn't specify any constructors so the empty constructor exists
    /// </summary>
    public class ValidBenefit : ILoggedInBenefit
    {
        public virtual bool Enabled => true;
        public virtual string EcopediaPageName => "Valid Benefit";
        public virtual float EcopediaPagePriority => 3;

        public void ApplyBenefitToUser(User user)
        {
        }
        public void RemoveBenefitFromUser(User user)
        {
        }
    }
    public class BenefitWithEcopediaPage : BenefitBase
    {
        public override string EcopediaPageName { get; } = "Ecopedia Benefit";
        public override float EcopediaPagePriority { get; } = 4;
        protected override LocString BenefitDescription { get; } = LocString.Empty;
        public override void ApplyBenefitToUser(User user)
        {
        }
        public override void RemoveBenefitFromUser(User user)
        {
        }
    }
    public class BenefitWithEcopediaPageEcopediaGenerator : BenefitEcopediaGenerator
    {
        protected override Type BenefitType => base.BenefitType;
        public override string Summary => "Test Summary";
        public override LocString DisplayName => Localizer.DoStr("Test Benefit");
    }
    public class BenefitWithoutEcopediaPage : ValidBenefit
    {
    }
    public class DisabledBenefit : ValidBenefit
    {
        public override bool Enabled => false;
        public override string EcopediaPageName => "Disabled Benefit";
    }
    public class NoDefaultConstructorBenefit : ValidBenefit
    {
        public NoDefaultConstructorBenefit(bool b) { }
    }
    public abstract class AbstractBenefit : ValidBenefit
    {
        public AbstractBenefit() { }
    }
    /// <summary>
    /// Should be ignored when finding benefits to instantiate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericBenefit<T> : ValidBenefit { }
}
namespace EcoTests
{
    public class Assert
    {
        public static void AreEqual(object expected, object actual)
        {
            if (!Equals(expected, actual))
            {
                throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}");
            }
        }
        public static void AreEqual(float expected, float actual, float delta = 0.0001f)
        {
            if (Math.Abs(expected - actual) > delta)
            {
                throw new Exception($"AreEqual failed.\nExpected={expected}\nActual={actual}\nwith difference no greater than {delta}");
            }
        }
        public static void AreNotEqual(object notExpected, object actual)
        {
            if (Equals(notExpected, actual))
            {
                throw new Exception($"AreNotEqual failed.\nNot Expected={notExpected}\nActual={actual}");
            }
        }
        public static void IsNull(object obj)
        {
            if (obj is not null)
            {
                throw new Exception($"IsNull failed.\nGot={obj}");
            }
        }
        public static void IsNotNull(object obj)
        {
            if (obj is null)
            {
                throw new Exception($"IsNotNull failed.");
            }
        }
        public static void IsTrue(bool value)
        {
            if (!value)
            {
                throw new Exception($"IsTrue failed.");
            }
        }
        public static void IsFalse(bool value)
        {
            if (value)
            {
                throw new Exception($"IsFalse failed.");
            }
        }
    }
    public static class Test
    {
        /// <summary>
        /// Unhandled exceptions in tests will cause the server to shut down and not run
        /// any remaining tests, so we need to catch any exceptions the tests throw
        /// </summary>
        /// <param name="test"></param>
        public static void Run(Action test)
        {
            float skillGainMultiplier = DifficultySettings.SkillGainMultiplier;
            DifficultySettings.SkillGainMultiplier = 1;
            try
            {
                test();
            }
            catch (Exception ex)
            {
                Log.WriteException(ex);
            }
            finally
            {
                DifficultySettings.SkillGainMultiplier = skillGainMultiplier;
            }
        }
    }
}
