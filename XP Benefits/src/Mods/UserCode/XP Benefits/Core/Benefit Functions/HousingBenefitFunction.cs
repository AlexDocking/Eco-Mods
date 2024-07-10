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
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using System;

namespace XPBenefits
{
    public class RegisterHousingBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new HousingBenefitFunctionFactory());
        }
    }

    public class HousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "HousingOnly";
        public string Description { get; } = "Uses only the amount of housing xp the player has.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            HousingXPInput input = new HousingXPInput(xpConfig);
            SimpleBenefitFunction benefitFunction = new SimpleBenefitFunction(input, maximumBenefit, xpLimitEnabled);
            LocString inputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"))} multiplier");
            benefitFunction.Describer = new InputDescriber(input)
            {
                InputName = "housing XP",
                InputTitle = inputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {inputTitle}"),
            };
            return benefitFunction;
        }
    }
}