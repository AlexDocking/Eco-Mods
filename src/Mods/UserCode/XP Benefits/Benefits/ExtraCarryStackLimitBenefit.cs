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
using CompatibleTools;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Eco.Mods.Organisms;
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;

namespace XPBenefits
{
    public class RegisterExtraCarryStackLimitBenefit : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "Mods";
        public string GetStatus() => "";
        public void Initialize(TimedTask timer)
        {
            var benefit = new ExtraCarryStackLimitBenefit();
            IBenefitDescriber benefitDescriber = new ExtraCarryStackLimitBenefitDescriber(benefit);
            ExtraCarryStackLimitTooltipLibrary.BenefitDescriber = benefitDescriber;
            var ecopediaGenerator = new ExtraCarryStackLimitEcopediaGenerator(benefit, benefitDescriber);
            XPBenefitsPlugin.RegisterBenefit(benefit);
            XPBenefitsEcopediaManager.Obj.RegisterEcopediaPageGenerator(benefit, ecopediaGenerator);
            UserManager.OnUserLoggedIn.Add(user => { if (benefit.Enabled) benefit.ApplyBenefitToUser(user); });
            UserManager.OnUserLoggedOut.Add(user => { if (benefit.Enabled) benefit.RemoveBenefitFromUser(user); });
        }
    }
    public partial class ExtraCarryStackLimitBenefit : BenefitBase
    {
        public override bool Enabled => XPConfig.ExtraCarryStackLimitEnabled;
        /// <summary>
        /// Used by shovels to work out how much their size should increase
        /// </summary>
        public IBenefitFunction ShovelBenefit { get; set; }

        public override void OnPluginLoaded()
        {
            base.OnPluginLoaded();
            XPConfig = XPBenefitsPlugin.Obj.Config;
            MaxBenefitValue = XPConfig.ExtraCarryStackLimitMaxBenefitValue;
            XPLimitEnabled = XPConfig.ExtraCarryStackLimitXPLimitEnabled;
            ModsPreInitialize();
            BenefitFunction = CreateBenefitFunction(XPConfig.ExtraCarryStackLimitBenefitFunction);
            ModsPostInitialize();
            if (!Enabled) return;
            ShovelBenefit ??= BenefitFunction;
            ShovelItem.MaxTakeResolver.Add(new ExtraCarryStackLimitModifier());
            TreeEntity.MaxPickupResolver.Add(new ExtraCarryStackLimitModifier());
            MiningSweepingHandsTalent.MaxStackSizeResolver.Add(new ExtraCarryStackLimitModifier());
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
    public class ExtraCarryStackLimitEcopediaGenerator : BenefitEcopediaGenerator
    {
        public ExtraCarryStackLimitEcopediaGenerator(BenefitBase benefit, IBenefitDescriber benefitDescriber) : base(benefit, benefitDescriber)
        {
        }
        public override LocString DisplayName { get; } = Localizer.DoStr("Bigger Hands");
        public override string Summary { get; } = "Earn extra carry capacity, so you can hold more blocks in your hands.";
        public override string IconName { get; } = "HandsItem";
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
        public override LocString BenefitDescription { get; } = Localizer.DoStr("extra carry capacity");
        public override string PageName { get; } = "Bigger Hands";
        public override float PagePriority { get; } = -6;
    }
    public class ExtraCarryStackLimitBenefitDescriber : IBenefitDescriber
    {
        ExtraCarryStackLimitBenefit Benefit { get; }

        public ExtraCarryStackLimitBenefitDescriber(ExtraCarryStackLimitBenefit benefit)
        {
            Benefit = benefit;
        }
        IBenefitFunction BenefitFunction => Benefit.BenefitFunction;
        public LocString MaximumBenefit(User user)
        {
            float maxBenefit = Benefit.MaxBenefitValue.GetValue(user);
            return TextLoc.StyledNumLoc(maxBenefit, maxBenefit.ToString("+0%;-0%"));
        }
        public LocString CurrentBenefit(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return TextLoc.StyledNumLoc(currentBenefit, currentBenefit.ToString("+0%;-0%"));
        }
        public LocString CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("+0%;-0%"), new Eco.Shared.Math.Range(0, Benefit.MaxBenefitValue.GetValue(user)));
        }
        public LocString CurrentInput(User user) => BenefitFunction.Describer.CurrentInput(user);
        public LocString InputName(User user) => BenefitFunction.Describer.InputName(user);
        public LocString MaximumInput(User user) => BenefitFunction.Describer.MaximumInput(user);
        public LocString MeansOfImprovingStat(User user) => BenefitFunction.Describer.MeansOfImprovingStat(user);
    }
    public partial class XPConfig
    {
        private string extraCarryStackLimitBenefitFunction = "GeometricMeanFoodHousing";

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra carry capacity. Requires restart.")]
        public bool ExtraCarryStackLimitEnabled { get; set; } = true;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Max Extra Carry Capacity"), LocDescription("How much extra carry stack size (hands slot) can be earned. " +
            "A value of 1 represents a 100% increase in stack limit for the items held in the hands e.g. carry 40 bricks instead of 20. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCarryStackLimitMaxBenefitValue { get; set; } = 1;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCarryStackLimitXPLimitEnabled { get; set; } = false;

        [Category("Benefit - Extra Carry Stack Limit"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public string ExtraCarryStackLimitBenefitFunction { get => XPBenefitsPlugin.Obj.ValidateBenefitFunctionType(extraCarryStackLimitBenefitFunction); set { extraCarryStackLimitBenefitFunction = value; } }
    }
    public class ExtraCarryStackLimitModifier : IPriorityModifyInPlaceDynamicValueHandler
    {
        public float Priority { get; } = 100;
        public void ModifyValue(IModifyInPlaceDynamicValueContext context)
        {
            var benefit = XPBenefitsPlugin.Obj.GetBenefit<ExtraCarryStackLimitBenefit>();
            if (benefit == null || !benefit.Enabled) return;
            float multiplier = 1 + benefit.ShovelBenefit.CalculateBenefit(context.User);
            Log.WriteLine(Localizer.Do($"Benefit multiplier:{multiplier},initial:{context.FloatValue},result:{context.FloatValue * multiplier}"));
            context.FloatValue *= multiplier;
            context.IntValue = (int)context.FloatValue;
        }
    }
    [TooltipLibrary]
    public static class ExtraCarryStackLimitTooltipLibrary
    {
        public static IBenefitDescriber BenefitDescriber { get; set; }
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 14)]
        public static LocString ExtraCarryStackLimitShovelTooltip(this ShovelItem shovel, User user)
        {
            var benefit = XPBenefitsPlugin.Obj.GetBenefit<ExtraCarryStackLimitBenefit>();
            if (benefit == null || !benefit.Enabled) return LocString.Empty;
            var ecopediaGenerator = XPBenefitsEcopediaManager.Obj.GetEcopedia(benefit);
            var currentBenefit = BenefitDescriber.CurrentBenefit(user);
            var ecopediaLink = ecopediaGenerator.GetPageLink();
            return new TooltipSection(Localizer.Do($"Shovel limit increased by {currentBenefit} due to {ecopediaLink}"));
        }
    }
}
