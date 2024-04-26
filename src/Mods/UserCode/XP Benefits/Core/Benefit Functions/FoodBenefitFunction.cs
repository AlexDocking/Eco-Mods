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
using static XPBenefits.BenefitDescriptionResolverStrings;

namespace XPBenefits
{
    public class RegisterFoodBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new FoodBenefitFunctionFactory());
        }
    }
    /// <summary>
    /// Scale the benefit by the amount of food xp the player has
    /// </summary>
    public class FoodBenefitFunction : IBenefitFunction
    {
        public XPConfig XPConfig { get; set; }
        public bool XPLimitEnabled { get; set; }
        public BenefitValue MaximumBenefit { get; set; }
        public FoodBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            XPConfig = xpConfig;
            XPLimitEnabled = xpLimitEnabled;
            MaximumBenefit = maximumBenefit;
        }
        public float CalculateBenefit(User user)
        {
            try
            {
                float fractionOfBenefitToApply = SkillRateUtil.FractionFoodXP(user, XPConfig, XPLimitEnabled);
                if (XPLimitEnabled)
                {
                    fractionOfBenefitToApply = Math.Min(1, fractionOfBenefitToApply);
                }
                return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
            }
            catch
            {
            }
            return 0;
        }
        private static string NutritionEcopediaPageLink => Ecopedia.Obj.GetPage("Nutrition").UILink();
        public virtual LocString ResolveToken(User user, string token)
        {
            switch (token)
            {
                case INPUT_NAME:
                    return Localizer.Do($"{NutritionEcopediaPageLink} multiplier");
                case MEANS_OF_IMPROVING_STAT:
                    return Localizer.Do($"You can increase this benefit by improving your {NutritionEcopediaPageLink} multiplier. Note that 'Base Gain' is ignored when calculating your nutrition percentage");
                case MAXIMUM_INPUT:
                    return Localizer.Do($"{TextLoc.StyledNumLoc(XPConfig.MaximumFoodXP, XPConfig.MaximumFoodXP.ToString("0.#"))} food XP");
                case MAXIMUM_BENEFIT:
                    return TextLoc.StyledNum(MaximumBenefit.GetValue(user));
                case CURRENT_INPUT:
                    return Localizer.Do($"{DisplayUtils.GradientNumLoc(SkillRateUtil.FoodXP(user), SkillRateUtil.FoodXP(user).ToString("0.#"), new Eco.Shared.Math.Range(XPConfig.BaseFoodXP, XPConfig.MaximumFoodXP))} food XP");
                case CURRENT_BENEFIT:
                    return TextLoc.StyledNum(CalculateBenefit(user));
                case CURRENT_BENEFIT_ECOPEDIA:
                    float currentBenefit = CalculateBenefit(user);
                    return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("0.#"), new Eco.Shared.Math.Range(0, MaximumBenefit.GetValue(user)));
                default:
                    return LocString.Empty;
            }
        }
    }
    public interface IBenefitFunctionFactory
    {
        string Name { get; }
        string Description { get; }
        IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false);
    }
    public class FoodBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "FoodOnly";
        public string Description { get; } = "Uses only the amount of food xp the player has.";
        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            return new FoodBenefitFunction(xpConfig, maximumBenefit, xpLimitEnabled);
        }
    }
}
