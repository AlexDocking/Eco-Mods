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
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    public class RegisterGeometricMeanFoodHousingBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new GeometricMeanFoodHousingBenefitFunctionFactory());
        }
    }

    /// <summary>
    /// Scale the benefit by the amount of food and housing xp the player has
    /// in such a way as to require both sources of xp to give any benefit
    /// </summary>
    public class GeometricMeanFoodHousingBenefitFunction : IBenefitFunction, IBenefitInputDescriber
    {
        public XPConfig XPConfig { get; set; }
        public bool XPLimitEnabled { get; set; }
        public BenefitValue MaximumBenefit { get; set; }
        private List<IBenefitFunctionInput> Inputs { get; set; }
        private List<InputDescriber> InputDescribers { get; set; }

        public GeometricMeanFoodHousingBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            XPConfig = xpConfig;
            XPLimitEnabled = xpLimitEnabled;
            MaximumBenefit = maximumBenefit;

            FoodXPInput foodInput = new FoodXPInput(xpConfig);
            HousingXPInput housingInput = new HousingXPInput(xpConfig);
            Inputs = new List<IBenefitFunctionInput>()
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
            InputDescribers = new List<InputDescriber>()
            {
                foodInputDescriber,
                housingInputDescriber
            };
        }

        public float CalculateBenefit(User user)
        {
            try
            {
                float product = Inputs.Select(input => input.GetScaledInput(user)).Mult();
                float fractionOfBenefitToApply = (float)Math.Pow(product, 1f / Inputs.Count);
                return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
            }
            catch
            {
            }
            return 0;
        }

        #region IBenefitInputDescriber

        public IBenefitInputDescriber Describer => this;

        public LocString InputName(User user) => Localizer.Do($"{InputDescribers.Select(describer => (describer as IBenefitInputDescriber).InputName(user)).CommaList()} {Localizer.PluralNoNum("multiplier")}");

        public LocString MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {InputDescribers.Select(describer => (describer as IBenefitInputDescriber).InputName(user)).CommaList()} {Localizer.PluralNoNum("multiplier")}. If you want to see the greatest improvement you should improve the lowest percentage first. {InputDescribers.Select(describer => describer.AdditionalInfo).Where(s => s.IsSet()).JoinList(". ")}");

        public LocString MaximumInput(User user)
        {
            List<LocString> locs = new List<LocString>();
            for (int i = 0; i < Inputs.Count; i++)
            {
                locs.Add(Localizer.Do($"{Text.StyledNum(Inputs[i].GetInputRange(user).Max)} {InputDescribers[i].InputName}"));
            }
            return locs.CommaList();
        }

        public LocString CurrentInput(User user)
        {
            List<LocString> locs = new List<LocString>();
            for (int i = 0; i < Inputs.Count; i++)
            {
                locs.Add(Localizer.Do($"{Text.GradientColoredPercent(Inputs[i].GetScaledInput(user))} {InputDescribers[i].InputName}"));
            }
            return locs.CommaList();
        }

        #endregion IBenefitInputDescriber
    }

    public class GeometricMeanFoodHousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name => "GeometricMeanFoodHousing";
        public string Description => "Uses a combination of the amount of food and housing xp the player has in such a way as to require both sources of xp to give any benefit.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            return new GeometricMeanFoodHousingBenefitFunction(xpConfig, maximumBenefit, xpLimitEnabled);
        }
    }
}