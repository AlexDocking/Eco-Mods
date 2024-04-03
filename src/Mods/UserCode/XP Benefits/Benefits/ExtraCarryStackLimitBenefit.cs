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
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using System.ComponentModel;
using System.Linq;

namespace XPBenefits
{
    [Benefit]
    public partial class ExtraCarryStackLimitBenefit : BenefitBase
    {
        public override bool Enabled => XPConfig.ExtraCarryStackLimitBenefitEnabled;
        /// <summary>
        /// Used by shovels to work out how much their size should increase
        /// </summary>
        public static IBenefitFunction ShovelBenefit { get; set; }
        public ExtraCarryStackLimitBenefit()
        {
            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraCarryStackLimitBenefitMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraCarryStackLimitBenefitXPLimitEnabled;
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraCarryStackLimitBenefitFunctionType, MaxBenefitValue, XPLimitEnabled);
            ShovelBenefit ??= BenefitFunction;
            ModsPostInitialize();
        }
        partial void ModsPreInitialize();
        partial void ModsPostInitialize();

        public override void ApplyBenefitToUser(User user)
        {
            var carryInventory = user.Inventory.Carried;

            if (!carryInventory.Restrictions.Any(restriction => restriction is StackLimitBenefitInventoryRestriction))
            {
                carryInventory.AddInvRestriction(new StackLimitBenefitInventoryRestriction(user, BenefitFunction));
            }
        }
        public override void RemoveBenefitFromUser(User user)
        {
            Inventory carryInventory = user.Inventory.Carried;
            carryInventory.RemoveAllRestrictions(restriction => restriction is StackLimitBenefitInventoryRestriction);
        }
    }
    public partial class XPConfig
    {
        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra carry capacity. Requires restart.")]
        public bool ExtraCarryStackLimitBenefitEnabled { get; set; } = true;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Max Extra Carry Capacity"), LocDescription("How much extra carry stack size (hands slot) can be earned. " +
            "A value of 1 represents a 100% increase in stack limit for the items held in the hands e.g. carry 40 bricks instead of 20. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCarryStackLimitBenefitMaxBenefitValue { get; set; } = 1;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCarryStackLimitBenefitXPLimitEnabled { get; set; } = false;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public BenefitFunctionType ExtraCarryStackLimitBenefitFunctionType { get; set; }
    }
}
