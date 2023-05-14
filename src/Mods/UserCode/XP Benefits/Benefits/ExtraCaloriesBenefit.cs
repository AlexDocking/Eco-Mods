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
    public partial class ExtraCaloriesBenefit : BenefitBase
    {
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        public ExtraCaloriesBenefit()
        {
            XPConfig = XPConfig.Obj;
            //The player can earn 6000 calories at max food and housing
            MaxBenefitValue = 6000;
            XPLimitEnabled = false;
            ModsPreInitialize();
            BenefitFunction = new GeometricMeanFoodHousingBenefitFunction(XPConfig, MaxBenefitValue, XPLimitEnabled);
            ModsPostInitialize();
        }
        /// <summary>
        /// Override to change how much extra calorie space the player can earn
        /// </summary>
        partial void ModsPreInitialize();
        /// <summary>
        /// Override to change how the amount of benefit is calculated from a user
        /// </summary>
        partial void ModsPostInitialize();

        public override void ApplyBenefitToUser(User user)
        {
            IDynamicValue benefit = new BenefitDynamicValue(BenefitFunction);

            Action updateStomachCapacity = user.Stomach.ChangedMaxCalories;
            ModifiersRegister.AddModifierToUser(user, UserStatType.MaxCalories, benefit, updateStomachCapacity);
        }

        public override void RemoveBenefitFromUser(User user)
        {
            ModifiersRegister.RemoveStatModifiersFromUser(user, UserStatType.MaxCalories);
        }
    }
}
