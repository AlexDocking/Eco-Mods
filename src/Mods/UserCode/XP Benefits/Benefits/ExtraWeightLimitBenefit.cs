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
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using System;

namespace XPBenefits
{
    public partial class ExtraWeightLimitBenefit : BenefitBase
    {
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        public ExtraWeightLimitBenefit()
        {
            XPConfig = XPConfig.Obj;
            //30kg extra carry weight
            MaxBenefitValue = 30000;
            XPLimitEnabled = false;
            ModsPreInitialize();
            BenefitFunction = new GeometricMeanFoodHousingBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
            ModsPostInitialize();
        }
        partial void ModsPreInitialize();
        partial void ModsPostInitialize();

        public override void ApplyBenefitToUser(User user)
        {
            IDynamicValue benefit = new BenefitDynamicValue(BenefitFunction);

            Action updateCarryWeight = user.ChangedCarryWeight;
            ModifiersRegister.AddModifierToUser(user, UserStatType.MaxCarryWeight, benefit, updateCarryWeight);
        }

        public override void RemoveBenefitFromUser(User user)
        {
        }
    }
}
