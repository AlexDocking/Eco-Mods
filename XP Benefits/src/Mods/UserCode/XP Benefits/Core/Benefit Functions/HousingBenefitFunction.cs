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
    /// <summary>
    /// Scale the benefit by the amount of housing xp the player has
    /// </summary>
    public class HousingBenefitFunction : IBenefitFunction, IBenefitInputDescriber
    {
        public XPConfig XPConfig { get; set; }
        public bool XPLimitEnabled { get; set; }
        public BenefitValue MaximumBenefit { get; set; }
        public HousingBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            XPConfig = xpConfig;
            XPLimitEnabled = xpLimitEnabled;
            MaximumBenefit = maximumBenefit;
        }
        public float CalculateBenefit(User user)
        {
            try
            {
                float fractionOfBenefitToApply = SkillRateUtil.FractionHousingXP(user, XPConfig, XPLimitEnabled);

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
        private static string HousingEcopediaPageLink => Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"));
        #region IBenefitInputDescriber
        public IBenefitInputDescriber Describer => this;
        public LocString InputName(User user) => Localizer.Do($"{HousingEcopediaPageLink} multiplier");
        public LocString MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {HousingEcopediaPageLink} multiplier");
        public LocString MaximumInput(User user) => Localizer.Do($"{TextLoc.StyledNum(XPConfig.MaximumFoodXP)} housing XP");
        public LocString CurrentInput(User user) => Localizer.Do($"{TextLoc.StyledNum(SkillRateUtil.HousingXP(user))} housing XP");
        #endregion
    }
    public class HousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "HousingOnly";
        public string Description { get; } = "Uses only the amount of housing xp the player has.";
        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            return new HousingBenefitFunction(xpConfig, maximumBenefit, xpLimitEnabled);
        }
    }
}
