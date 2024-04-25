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
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Shared.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using static XPBenefits.BenefitDescriptionResolverStrings;

namespace XPBenefits
{
    public partial class ExtraCaloriesBenefit : BenefitBase
    {
        public override bool Enabled => XPConfig.ExtraCaloriesEnabled;
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();
        public override BenefitEcopediaGenerator EcopediaGenerator { get; }

        public ExtraCaloriesBenefit()
        {
            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraCaloriesMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraCaloriesXPLimitEnabled;
            EcopediaGenerator = new ExtraCaloriesEcopediaGenerator(this);
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraCaloriesBenefitFunctionType);
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
            if (BenefitFunction == null) return;
            IDynamicValue benefit = new BenefitDynamicValue(BenefitFunction);

            Action updateStomachCapacity = user.Stomach.ChangedMaxCalories;
            ModifiersRegister.AddModifierToUser(user, UserStatType.MaxCalories, benefit, updateStomachCapacity);
        }
        public override void RemoveBenefitFromUser(User user) { }
    }
    public class ExtraCaloriesEcopediaGenerator : BenefitEcopediaGenerator
    {
        public ExtraCaloriesEcopediaGenerator(BenefitBase benefit) : base(benefit)
        {
        }

        public override string PageName { get; } = "Expandable Stomach";
        public override float PagePriority { get; } = -5;
        public override LocString DisplayName { get; } = Localizer.DoStr("Expandable Stomach");
        public override string Summary { get; } = "Earn extra calorie space, so you can eat more food before you get full.";
        public override string IconName { get; } = "Ecopedia_FoodandShelter";
        public override IEnumerable<LocString> Sections
        {
            get
            {
                List<LocString> sections = new List<LocString>();
                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Benefit Description"));
                locStringBuilder.AppendLineLoc($"You can earn extra calorie space, so you can eat more food before you get full.");
                sections.Add(locStringBuilder.ToLocString());
                return sections;
            }
        }
        public override LocString BenefitDescription => Localizer.DoStr("extra calorie space");
        public override LocString ResolveToken(User user, string token)
        {
            float currentBenefit;
            switch (token)
            {
                case MAXIMUM_BENEFIT:
                    float maxBenefit = Benefit.MaxBenefitValue.GetValue(user);
                    return TextLoc.StyledNumLoc(maxBenefit, maxBenefit.ToString("+0;-0"));
                case CURRENT_BENEFIT:
                    currentBenefit = Benefit.BenefitFunction.CalculateBenefit(user);
                    return TextLoc.StyledNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"));
                case CURRENT_BENEFIT_ECOPEDIA:
                    currentBenefit = Benefit.BenefitFunction.CalculateBenefit(user);
                    return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"), new Eco.Shared.Math.Range(0, Benefit.MaxBenefitValue.GetValue(user)));
                default:
                    return base.ResolveToken(user, token);
            }
        }
    }
    public partial class XPConfig
    {
        private string extraCaloriesBenefitFunctionType;

        [Category("Benefit - Extra Calories"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra calorie capacity. Requires restart.")]
        public bool ExtraCaloriesEnabled { get; set; } = true;

        [Category("Benefit - Extra Calories"), LocDisplayName("Max Extra Calories"), LocDescription("How much extra calorie space can be earned. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCaloriesMaxBenefitValue { get; set; } = 6000;

        [Category("Benefit - Extra Calories"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCaloriesXPLimitEnabled { get; set; } = false;

        [Category("Benefit - Extra Calories"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public string ExtraCaloriesBenefitFunctionType
        {
            get => extraCaloriesBenefitFunctionType; set
            {
                extraCaloriesBenefitFunctionType = XPBenefitsPlugin.Obj.ValidateBenefitFunctionType(value);
            }
        }
    }
    [TooltipLibrary]
    public static class ExtraCaloriesTooltipLibrary
    {
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 103, overrideType: typeof(Stomach))]
        public static LocString ExtraCaloriesStomachTooltip(this Stomach stomach)
        {
            ExtraCaloriesBenefit benefit = XPBenefitsPlugin.Obj.GetBenefit<ExtraCaloriesBenefit>();
            if (benefit == null || !benefit.Enabled)
            {
                return LocString.Empty;
            }
            User user = stomach.Owner;
            LocString extraWeightLimit = benefit.EcopediaGenerator.ResolveToken(user, CURRENT_BENEFIT);
            return new TooltipSection(Localizer.Do($"Calorie limit boosted by {extraWeightLimit} due to {benefit.BenefitEcopedia.GetPageLink()}."));
        }
    }
}
