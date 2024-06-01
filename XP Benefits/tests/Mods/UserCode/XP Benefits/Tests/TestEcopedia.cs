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
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using System.Linq;
using Eco.Gameplay.EcopediaRoot;
using EcoTestTools;

namespace XPBenefits.Tests
{
    [ChatCommandHandler]
    public static class TestEcopedia
    {
        [ChatCommand(ChatAuthorizationLevel.Developer)]
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
