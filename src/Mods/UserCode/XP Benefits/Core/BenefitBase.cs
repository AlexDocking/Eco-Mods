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
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections;
using System.Collections.Generic;
using static XPBenefits.BenefitDescriptionResolverStrings;

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
        public virtual string EcopediaPageName { get; } = string.Empty;
        public virtual float EcopediaPagePriority { get; } = 0;
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
        protected abstract LocString BenefitDescription { get; }

        public virtual LocString ResolveToken(User user, string token) => BenefitFunction.ResolveToken(user, token);
        public virtual LocString GenerateEcopediaDescription(User user)
        {
            var locStringBuilder = new LocStringBuilder();
            LocString Resolve(string token) => ResolveToken(user, token);

            locStringBuilder.AppendLineLoc($"{Resolve(MEANS_OF_IMPROVING_STAT)}.");
            locStringBuilder.AppendLine();
            if (XPLimitEnabled)
            {
                locStringBuilder.AppendLineLoc($"With {Resolve(MAXIMUM_INPUT)} you would receive {Resolve(MAXIMUM_BENEFIT)} {BenefitDescription}. Note that the benefit will be capped at this.");
            }
            else
            {
                locStringBuilder.AppendLineLoc($"With {Resolve(MAXIMUM_INPUT)} you would receive {Resolve(MAXIMUM_BENEFIT)} {BenefitDescription}. Note that the benefit will not be capped at this if you can do even better.");
            }
            locStringBuilder.AppendLine();
            locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Current Status"));
            locStringBuilder.AppendLineLoc($"You have {Resolve(CURRENT_INPUT)}, which is providing you with {Resolve(CURRENT_BENEFIT_ECOPEDIA)} {BenefitDescription}.");

            return locStringBuilder.ToLocString();
        }
    }
}
