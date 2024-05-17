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
using Eco.Shared.Math;

namespace XPBenefits
{
    public class RegisterSkillRateBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new SkillRateBenefitFunctionFactory());
        }
    }
    /// <summary>
    /// Scale the benefit by the amount of food and housing xp the player has,
    /// treating both sources of xp equally
    /// </summary>
    public class SkillRateBenefitFunction : IBenefitFunction, IBenefitDescriber
    {
        public XPConfig XPConfig { get; set; }
        public bool XPLimitEnabled { get; set; }
        public BenefitValue MaximumBenefit { get; set; }
        public SkillRateBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            XPConfig = xpConfig;
            XPLimitEnabled = xpLimitEnabled;
            MaximumBenefit = maximumBenefit;
        }
        public float CalculateBenefit(User user)
        {
            try
            {
                float housingXP = SkillRateUtil.AdjustedHousingXP(user, XPConfig, XPLimitEnabled);
                float foodXP = SkillRateUtil.AdjustedFoodXP(user, XPConfig, XPLimitEnabled);
                float fractionOfBenefitToApply = (foodXP + housingXP) / (XPConfig.AdjustedMaximumFoodXP + XPConfig.AdjustedMaximumHousingXP);
                return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
            }
            catch
            {
            }
            return 0;
        }
        private string ExperienceEcopediaPageLink => Ecopedia.Obj.GetPage("Experience").UILink(Localizer.DoStr("XP Multiplier"));
        #region IBenefitDescriber
        IBenefitDescriber IBenefitFunction.Describer => this;
        LocString IBenefitDescriber.InputName(User user) => Localizer.Do($"{ExperienceEcopediaPageLink}");
        LocString IBenefitDescriber.MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {ExperienceEcopediaPageLink}");
        LocString IBenefitDescriber.MaximumInput(User user)
        {
            float maxXPMultiplier = XPConfig.MaximumFoodXP + XPConfig.MaximumHousingXP;
            return Localizer.Do($"an XP multiplier of {TextLoc.StyledNumLoc(maxXPMultiplier, maxXPMultiplier.ToString("0.#"))}");
        }
        LocString IBenefitDescriber.MaximumBenefit(User user) => TextLoc.StyledNum(MaximumBenefit.GetValue(user));
        LocString IBenefitDescriber.CurrentInput(User user)
        {
            float housingXP = SkillRateUtil.HousingXP(user);
            float foodXP = SkillRateUtil.FoodXP(user);
            float xpMultiplier = foodXP + housingXP;
            return Localizer.Do($"an XP multiplier of {DisplayUtils.GradientNumLoc(xpMultiplier, xpMultiplier.ToString("0.#"), new Range(XPConfig.BaseFoodXP, XPConfig.MaximumFoodXP + XPConfig.MaximumHousingXP))}");
        }
        LocString IBenefitDescriber.CurrentBenefit(User user) => TextLoc.StyledNum(CalculateBenefit(user));
        LocString IBenefitDescriber.CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = CalculateBenefit(user);
            return Localizer.Do($"{DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("0.#"), new Eco.Shared.Math.Range(0, MaximumBenefit.GetValue(user)))}");
        }
        #endregion
    }
    public class SkillRateBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "SkillRate";
        public string Description { get; } = "Treats both food and housing xp equally after they have been scaled by their own maximums, taking their average. [100% food + 0% housing] and [0% food + 100% housing] are both equivalent to [50% food + 50% housing].";
        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            return new SkillRateBenefitFunction(xpConfig, maximumBenefit, xpLimitEnabled);
        }
    }
}
