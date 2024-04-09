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
using Eco.Core.Systems;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static XPBenefits.BenefitDescriptionResolverStrings;
using Eco.Gameplay.Items;
using System.Linq;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Gameplay.Items.Actionbar;
using Eco.Core.Controller;

namespace XPBenefits
{
    [Benefit]
    public partial class ExtraWeightLimitBenefit : BenefitBase
    {
        public static ExtraWeightLimitBenefit Obj { get; private set; }
        public override bool Enabled => XPConfig.ExtraWeightLimitBenefitEnabled;

        public override string EcopediaPageName => ECOPEDIA_PAGE_NAME;
        public const string ECOPEDIA_PAGE_NAME = "Stronger Back";
        public override float EcopediaPagePriority => ECOPEDIA_PAGE_PRIORITY;
        public const float ECOPEDIA_PAGE_PRIORITY = -4;
        protected override LocString BenefitDescription => Localizer.DoStr("extra carry weight capacity");

        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();

        public ExtraWeightLimitBenefit()
        {
            Obj = this;

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
        public override LocString ResolveToken(User user, string token)
        {
            float currentBenefit;
            switch (token)
            {
                case MAXIMUM_BENEFIT:
                    float maxBenefit = MaxBenefitValue.GetValue(user);
                    return TextLoc.StyledNumLoc(maxBenefit, (maxBenefit / 1000).ToString("+0.#;-0.#")) + "kg";
                case CURRENT_BENEFIT:
                    currentBenefit = BenefitFunction.CalculateBenefit(user);
                    return TextLoc.StyledNumLoc(currentBenefit, (currentBenefit / 1000).ToString("+0.#;-0.#")) + "kg";
                case CURRENT_BENEFIT_ECOPEDIA:
                    currentBenefit = BenefitFunction.CalculateBenefit(user);
                    return DisplayUtils.GradientNumLoc(currentBenefit, (currentBenefit / 1000).ToString("+0.#;-0.#"), new Eco.Shared.Math.Range(0, MaxBenefitValue.GetValue(user))) + "kg";
                default:
                    return base.ResolveToken(user, token);
            }
        }

    }
    public class ExtraWeightLimitEcopediaGenerator : IEcopediaGeneratedData
    {
        private EcopediaPage CreateEcopediaPage()
        {
            const string pageName = ExtraWeightLimitBenefit.ECOPEDIA_PAGE_NAME;
            const float pagePriority = ExtraWeightLimitBenefit.ECOPEDIA_PAGE_PRIORITY;

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
                page.DisplayName = Localizer.DoStr("Stronger Back");
                page.Summary = Localizer.DoStr("Earn extra carry weight capacity, so you can keep more heavy items in your toolbar and backpack.");
                page.FullName = "XP Benefits;" + pageName;
                page.IconName = "BackpackItem";

                LocStringBuilder locStringBuilder = new LocStringBuilder();
                locStringBuilder.AppendLine(TextLoc.HeaderLoc($"Benefit Description"));
                locStringBuilder.AppendLineLoc($"You can earn extra carry weight capacity, so you can keep more heavy items in your toolbar and backpack.");
                var section = new Eco.Gameplay.EcopediaRoot.EcopediaSection();
                section.Text = locStringBuilder.ToLocString();
                page.Sections.Add(section);
                page.Changed(nameof(EcopediaPage.Sections));
                page.ParseTagsInText();
            }
            xpBenefitPages.Add(pageName, page);
            return page;
        }
        #region Ecopedia
        public virtual LocString GetEcopediaData(Player player, EcopediaPage page)
        {
            LocStringBuilder locStringBuilder = new LocStringBuilder();
            locStringBuilder.AppendLine(ExtraWeightLimitBenefit.Obj.GenerateEcopediaDescription(player.User));
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
    [TooltipLibrary]
    public static class ExtraWeightTooltipLibrary
    {
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 140, overrideType: typeof(BackpackItem))]
        public static LocString ExtraWeightLimitTooltip(User user)
        {
            if (!ExtraWeightLimitBenefit.Obj.Enabled)
            {
                return LocString.Empty;
            }
            //User user = ExtraWeightLimitBenefit.Obj.UsersWithBenefit.FirstOrDefault(onlineUser => onlineUser.Inventory.ToolbarBackpack == toolbarBackpack);
            if (user == null)
            {
                return Localizer.DoStr("Missing user in tooltip");
            }

            LocString extraWeightLimit = ExtraWeightLimitBenefit.Obj.ResolveToken(user, CURRENT_BENEFIT);
            return new TooltipSection(Localizer.Do($"Weight limit boosted by {extraWeightLimit} due to {Ecopedia.Obj.GetPage(ExtraWeightLimitBenefit.ECOPEDIA_PAGE_NAME).UILink()}."));
        }

        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 90, overrideType: typeof(ToolbarBackpackInventory))]
        public static LocString ExtraWeightLimitTooltip(this ToolbarBackpackInventory toolbarBackpack, User user) => ExtraWeightLimitTooltip(user);
    }
}
