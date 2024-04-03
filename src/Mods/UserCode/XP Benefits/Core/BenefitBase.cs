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
    public abstract class BenefitBase : ILoggedInBenefit
    {
        public virtual bool Enabled { get; } = true;
        /// <summary>
        /// Whether players can continue gaining benefits above those defined
        /// or whether their xp should be capped for the purposes of the calculation
        /// e.g. 55 food xp is the same as 50 if the config says the maximum food is 50.
        /// </summary>
        public virtual bool XPLimitEnabled { get; protected set; }
        public virtual XPConfig XPConfig { get; protected set; }
        public virtual BenefitValue MaxBenefitValue { get; protected set; }
        public virtual IBenefitFunction BenefitFunction { get; protected set; }

        public abstract void ApplyBenefitToUser(User user);
        public abstract void RemoveBenefitFromUser(User user);
        protected IBenefitFunction CreateBenefitFunction(BenefitFunctionType benefitFunctionType, BenefitValue maximumBenefit, bool xpLimitEnabled)
        {
            switch (benefitFunctionType)
            {
                case BenefitFunctionType.GeometricMeanFoodHousing:
                    return new GeometricMeanFoodHousingBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
                case BenefitFunctionType.FoodOnly:
                    return new FoodBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
                case BenefitFunctionType.HousingOnly:
                    return new HousingBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
                case BenefitFunctionType.SkillRate:
                    return new SkillRateBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
                default:
                    return null;
            }
        }
    }
}
