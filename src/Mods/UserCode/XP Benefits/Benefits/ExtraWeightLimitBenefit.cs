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
using Eco.Shared.Localization;
using System;
using System.ComponentModel;

namespace XPBenefits
{
    [Benefit]
    public partial class ExtraWeightLimitBenefit : BenefitBase
    {
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        public ExtraWeightLimitBenefit()
        {
            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraWeightLimitBenefitMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraWeightLimitBenefitXPLimitEnabled;
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraWeightLimitBenefitFunctionType, MaxBenefitValue, XPLimitEnabled);
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
    public partial class XPConfig
    {
        [Category("Benefit - Extra Weight Limit"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra backpack/toolbar inventory weight limit. Requires restart.")]
        public bool ExtraWeightLimitBenefitEnabled { get; set; } = true;

        [Category("Benefit - Extra Weight Limit"), LocDisplayName("Max Extra Weight Limit"), LocDescription("How much extra backpack/toolbar inventory weight limit can be earned, in grams (e.g. 30000 = +30kg). " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public int ExtraWeightLimitBenefitMaxBenefitValue { get; set; } = 30000;

        [Category("Benefit - Extra Weight Limit"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraWeightLimitBenefitXPLimitEnabled { get; set; } = false;
        
        [Category("Benefit - Extra Weight Limit"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public BenefitFunctionType ExtraWeightLimitBenefitFunctionType { get; set; }
    }
}
