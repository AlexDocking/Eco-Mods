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
using Eco.Gameplay.Civics;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static XPBenefits.BenefitDescriptionResolverStrings;

namespace XPBenefits
{
    [Benefit]
    public partial class ExtraCaloriesBenefit : BenefitBase
    {
        public static ExtraCaloriesBenefit Obj { get; private set; }
        public override bool Enabled => XPConfig.ExtraCaloriesEnabled;
        public override string EcopediaPageName => ECOPEDIA_PAGE_NAME;
        public const string ECOPEDIA_PAGE_NAME = "Expandable Stomach";
        public override float EcopediaPagePriority => ECOPEDIA_PAGE_PRIORITY;
        public const float ECOPEDIA_PAGE_PRIORITY = -5;
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        protected override LocString BenefitDescription => Localizer.DoStr("extra calorie space");

        internal ExtraCaloriesBenefit()
        {
            Obj = this;

            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraCaloriesMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraCaloriesXPLimitEnabled;
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraCaloriesBenefitFunctionType, MaxBenefitValue, XPLimitEnabled);
            ModsPostInitialize();
            Log.WriteLine(Localizer.DoStr("Extra Calories post initialize"));
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
        public override void RemoveBenefitFromUser(User user) { }
        public override LocString ResolveToken(User user, string token)
        {
            float currentBenefit;
            switch (token)
            {
                case MAXIMUM_BENEFIT:
                    float maxBenefit = MaxBenefitValue.GetValue(user);
                    return TextLoc.StyledNumLoc(maxBenefit, maxBenefit.ToString("+0;-0"));
                case CURRENT_BENEFIT:
                    currentBenefit = BenefitFunction.CalculateBenefit(user);
                    return TextLoc.StyledNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"));
                case CURRENT_BENEFIT_ECOPEDIA:
                    currentBenefit = BenefitFunction.CalculateBenefit(user);
                    return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"), new Eco.Shared.Math.Range(0, MaxBenefitValue.GetValue(user)));
                default:
                    return base.ResolveToken(user, token);
            }
        }
        public LocString ColouredCaloriesNumberLoc(User user, float extraCalories) => Localizer.DoStr(DisplayUtils.GradientNum(extraCalories, extraCalories, new Eco.Shared.Math.Range(0, MaxBenefitValue.GetValue(user))));
    }
    public class ExtraCaloriesEcopediaGenerator : IEcopediaGeneratedData
    {
        const string pageName = ExtraCaloriesBenefit.ECOPEDIA_PAGE_NAME;
        const float pagePriority = ExtraCaloriesBenefit.ECOPEDIA_PAGE_PRIORITY;
        #region Ecopedia
        private EcopediaPage CreateEcopediaPage()
        {
            Dictionary<string, EcopediaPage> xpBenefitPages = Ecopedia.Obj.Categories["XP Benefits"].Pages;
            if (xpBenefitPages.TryGetValue(pageName, out var existingPage))
            {
                Log.WriteLine(Localizer.Do($"{pageName} exists in category"));
                return existingPage;
            }
            var page = UnserializedNamedEntry<EcopediaPage>.GetByName(pageName);
            if (page == null)
            {
                page = new EcopediaPage();
                page.Name = pageName;
                page.Priority = pagePriority;
                page.DisplayName = Localizer.DoStr("Expandable Stomach");
                page.Summary = Localizer.DoStr("Earn extra calorie space, so you can eat more food before you get full.");
                page.FullName = "XP Benefits;" + pageName;
                page.IconName = "Ecopedia_FoodandShelter";

                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Benefit Description"));
                locStringBuilder.AppendLineLoc($"You can earn extra calorie space, so you can eat more food before you get full.");
                var section = new Eco.Gameplay.EcopediaRoot.EcopediaSection();
                section.Text = locStringBuilder.ToLocString();
                page.Sections.Add(section);
                page.Changed(nameof(EcopediaPage.Sections));
                page.ParseTagsInText();
            }
            xpBenefitPages.Add(pageName, page);
            return page;
        }
        public virtual LocString GetEcopediaData(Player player, EcopediaPage page)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.AppendLine(ExtraCaloriesBenefit.Obj.GenerateEcopediaDescription(player.User));
            return locStringBuilder.ToLocString();
        }
        public virtual IEnumerable<EcopediaPageReference> PagesWeSupplyDataFor()
        {
            EcopediaPage page = CreateEcopediaPage();
            return new EcopediaPageReference(null, "XP Benefits", page.Name, page.DisplayName).SingleItemAsEnumerable();
        }
        #endregion
    }
    public partial class XPConfig
    {
        [Category("Benefit - Extra Calories"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra calorie capacity. Requires restart.")]
        public bool ExtraCaloriesEnabled { get; set; } = true;

        [Category("Benefit - Extra Calories"), LocDisplayName("Max Extra Calories"), LocDescription("How much extra calorie space can be earned. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCaloriesMaxBenefitValue { get; set; } = 6000;

        [Category("Benefit - Extra Calories"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCaloriesXPLimitEnabled { get; set; } = false;
        
        [Category("Benefit - Extra Calories"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public BenefitFunctionType ExtraCaloriesBenefitFunctionType { get; set; }
    }
    [TooltipLibrary]
    public static class ExtraCaloriesTooltipLibrary
    {
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 103, overrideType: typeof(Stomach))]
        public static LocString ExtraCaloriesStomachTooltip(this Stomach stomach)
        {
            if (!ExtraCaloriesBenefit.Obj.Enabled)
            {
                return LocString.Empty;
            }
            User user = stomach.Owner;
            LocString extraWeightLimit = ExtraCaloriesBenefit.Obj.ResolveToken(user, CURRENT_BENEFIT);
            return new TooltipSection(Localizer.Do($"Calorie limit boosted by {extraWeightLimit} due to {Ecopedia.Obj.GetPage(ExtraCaloriesBenefit.ECOPEDIA_PAGE_NAME).UILink()}."));
        }
    }
}
