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
    public class GeometricMeanFoodHousingBenefitFunction : IBenefitFunction, IBenefitDescriber
    {
        public XPConfig XPConfig { get; set; }
        public bool XPLimitEnabled { get; set; }
        public BenefitValue MaximumBenefit { get; set; }
        public GeometricMeanFoodHousingBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            XPConfig = xpConfig;
            XPLimitEnabled = xpLimitEnabled;
            MaximumBenefit = maximumBenefit;
        }

        public float CalculateBenefit(User user)
        {
            try
            {
                float housingXP = SkillRateUtil.FractionHousingXP(user, XPConfig, XPLimitEnabled);
                float foodXP = SkillRateUtil.FractionFoodXP(user, XPConfig, XPLimitEnabled);
                float fractionOfBenefitToApply = (float)Math.Sqrt(housingXP * foodXP);
                return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
            }
            catch
            {
            }
            return 0;
        }
        private static string NutritionEcopediaPageLink => Ecopedia.Obj.GetPage("Nutrition").UILink();
        private static string HousingEcopediaPageLink => Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"));

        #region IBenefitDescriber
        IBenefitDescriber IBenefitFunction.Describer => this;
        LocString IBenefitDescriber.InputName(User user) => Localizer.Do($"{NutritionEcopediaPageLink} and {HousingEcopediaPageLink} multipliers");
        LocString IBenefitDescriber.MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {NutritionEcopediaPageLink} and {HousingEcopediaPageLink} multipliers. If you want to see the greatest improvement you should improve the lower of the two percentages first. Note that 'Base Gain' is ignored when calculating your food XP percentage");
        LocString IBenefitDescriber.MaximumInput(User user) => Localizer.Do($"{TextLoc.StyledNum(XPConfig.MaximumFoodXP)} food XP and {TextLoc.StyledNum(XPConfig.MaximumHousingXP)} housing XP");
        LocString IBenefitDescriber.MaximumBenefit(User user) => TextLoc.StyledNum(MaximumBenefit.GetValue(user));
        LocString IBenefitDescriber.CurrentInput(User user)
        {
            float housingXP = SkillRateUtil.FractionHousingXP(user, XPConfig, XPLimitEnabled);
            float foodXP = SkillRateUtil.FractionFoodXP(user, XPConfig, XPLimitEnabled);
            return Localizer.Do($"{Text.GradientColoredPercent(foodXP)} food XP and {Text.GradientColoredPercent(housingXP)} housing XP");
        }
        LocString IBenefitDescriber.CurrentBenefit(User user) => TextLoc.StyledNum(CalculateBenefit(user));
        LocString IBenefitDescriber.CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = CalculateBenefit(user);
            return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("0.#"), new Eco.Shared.Math.Range(0, MaximumBenefit.GetValue(user)));
        }
        #endregion
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