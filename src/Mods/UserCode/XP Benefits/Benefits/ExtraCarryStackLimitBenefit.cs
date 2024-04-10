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
using Eco.Core.Controller;
using Eco.Core.Systems;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static XPBenefits.BenefitDescriptionResolverStrings;

namespace XPBenefits
{
    public partial class ExtraCarryStackLimitBenefit : BenefitBase
    {
        public static ExtraCarryStackLimitBenefit Obj { get; private set; }
        public override bool Enabled => XPConfig.ExtraCarryStackLimitBenefitEnabled;
        public override string EcopediaPageName { get; } = "Bigger Hands";
        public override float EcopediaPagePriority { get; } = -6;
        protected override LocString BenefitDescription => Localizer.DoStr("extra carry capacity");
        /// <summary>
        /// Used by shovels to work out how much their size should increase
        /// </summary>
        public static IBenefitFunction ShovelBenefit { get; set; }
        public ExtraCarryStackLimitBenefit()
        {
            Obj = this;

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
        public override LocString ResolveToken(User user, string token)
        {
            float currentBenefit;
            switch (token)
            {
                case MAXIMUM_BENEFIT:
                    float maxBenefit = MaxBenefitValue.GetValue(user);
                    return TextLoc.StyledNumLoc(maxBenefit, maxBenefit.ToString("+0%;-0%"));
                case CURRENT_BENEFIT:
                    currentBenefit = MaxBenefitValue.GetValue(user);
                    return TextLoc.StyledNumLoc(currentBenefit, currentBenefit.ToString("+0%;-0%"));
                case CURRENT_BENEFIT_ECOPEDIA:
                    currentBenefit = BenefitFunction.CalculateBenefit(user);
                    return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("+0%;-0%"), new Eco.Shared.Math.Range(0, MaxBenefitValue.GetValue(user)));
                default:
                    return base.ResolveToken(user, token);
            }
        }
    }
    public class ExtraCarryStackLimitEcopediaGenerator : BenefitEcopediaGenerator
    {
        public override LocString DisplayName { get; } = Localizer.DoStr("Bigger Hands");
        public override string Summary { get; } = "Earn extra carry capacity, so you can hold more blocks in your hands.";
        public override string IconName { get; } = "HandsItem";
        protected override Type BenefitType { get; } = typeof(ExtraCarryStackLimitBenefit);
        public override IEnumerable<LocString> Sections
        {
            get
            {
                List<LocString> sections = new List<LocString>();
                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Benefit Description"));
                locStringBuilder.AppendLineLoc($"You can earn extra carry capacity, so you can hold more blocks in your hands.");
                sections.Add(locStringBuilder.ToLocString());
                return sections;
            }
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
        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("All Big Shovel"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool AllBigShovel { get; set; } = false;
    }

    [TooltipLibrary]
    public static class ExtraCarryStackLimitTooltipLibrary
    {
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 0, Eco.Shared.Items.TTCat.Controls)]
        public static LocString ExtraCarryStackLimitTooltip1(this BlockItem block)
        {
            return Localizer.DoStr("Extra Carry Stack Limit Tooltip Controls");
        }
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 0, Eco.Shared.Items.TTCat.Details)]
        public static LocString ExtraCarryStackLimitTooltip2(this BlockItem block)
        {
            return Localizer.DoStr("Extra Carry Stack Limit Tooltip details");
        }
    }
}
