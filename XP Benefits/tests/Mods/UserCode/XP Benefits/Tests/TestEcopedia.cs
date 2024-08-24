using Eco.Core.Tests;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using System.Linq;
using Eco.Gameplay.EcopediaRoot;
using EcoTestTools;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public static class TestEcopedia
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestEcopediaCreation()
        {
            Test.Run(ShouldCreateEcopediaPagesForBenefits);
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
    }
}
