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
using Eco.Gameplay.Civics;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems;
using Eco.Shared.Localization;
using System;
using System.ComponentModel;

namespace XPBenefits
{
    [Benefit]
    public partial class ExtraCaloriesBenefit : BenefitBase
    {
        public override bool Enabled => XPConfig.ExtraCaloriesBenefitEnabled;
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        public ExtraCaloriesBenefit()
        {
            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraCaloriesBenefitMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraCaloriesBenefitXPLimitEnabled;
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraCaloriesBenefitFunctionType, MaxBenefitValue, XPLimitEnabled);
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
        }
    }
    public partial class XPConfig
    {
        [Category("Benefit - Extra Calories"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra calorie capacity. Requires restart.")]
        public bool ExtraCaloriesBenefitEnabled { get; set; } = true;

        [Category("Benefit - Extra Calories"), LocDisplayName("Max Extra Calories"), LocDescription("How much extra calorie space can be earned. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCaloriesBenefitMaxBenefitValue { get; set; } = 6000;

        [Category("Benefit - Extra Calories"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCaloriesBenefitXPLimitEnabled { get; set; } = false;
        
        [Category("Benefit - Extra Calories"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public BenefitFunctionType ExtraCaloriesBenefitFunctionType { get; set; }
    }
}
