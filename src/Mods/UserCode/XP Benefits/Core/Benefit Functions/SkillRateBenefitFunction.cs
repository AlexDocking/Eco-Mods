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
using Eco.Gameplay.Players;

namespace XPBenefits
{
    /// <summary>
    /// Scale the benefit by the amount of food and housing xp the player has,
    /// treating both sources of xp equally
    /// </summary>
    public class SkillRateBenefitFunction : IBenefitFunction
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
    }
}
