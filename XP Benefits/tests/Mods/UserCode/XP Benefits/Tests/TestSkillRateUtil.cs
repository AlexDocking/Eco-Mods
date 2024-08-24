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