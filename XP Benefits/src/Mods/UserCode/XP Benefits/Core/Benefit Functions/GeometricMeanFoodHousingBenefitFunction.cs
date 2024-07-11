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
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using System.Collections.Generic;

namespace XPBenefits
{
    public class RegisterGeometricMeanFoodHousingBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new GeometricMeanFoodHousingBenefitFunctionFactory());
        }
    }

    public class GeometricMeanFoodHousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name => "GeometricMeanFoodHousing";
        public string Description => "Uses a combination of the amount of food and housing xp the player has in such a way as to require both sources of xp to give any benefit.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            FoodXPInput foodInput = new FoodXPInput(xpConfig);
            HousingXPInput housingInput = new HousingXPInput(xpConfig);
            List<IBenefitFunctionInput> inputs = new List<IBenefitFunctionInput>()
            {
                foodInput,
                housingInput
            };
            LocString foodInputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Nutrition").UILink()}");
            InputDescriber foodInputDescriber = new InputDescriber(foodInput)
            {
                InputName = "food XP",
                InputTitle = foodInputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {foodInputTitle}"),
                AdditionalInfo = Localizer.DoStr("Note that 'Base Gain' is ignored when calculating your nutrition percentage"),
            };
            LocString housingInputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"))}");
            InputDescriber housingInputDescriber = new InputDescriber(housingInput)
            {
                InputName = "housing XP",
                InputTitle = housingInputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {housingInputTitle}"),
            };
            List<InputDescriber> inputDescribers = new List<InputDescriber>()
            {
                foodInputDescriber,
                housingInputDescriber
            };
            return new GeometricMeanBenefitFunction(inputs, inputDescribers, maximumBenefit);
        }
    }
}