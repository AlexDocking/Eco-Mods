using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.NewTooltip;
using Eco.Gameplay.Systems.NewTooltip.TooltipLibraryFiles;
using Eco.Shared.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace XPBenefits
{
    public class RegisterExtraCaloriesBenefit : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "Mods";
        public string GetStatus() => "";
        public void Initialize(TimedTask timer)
        {
            var benefit = new ExtraCaloriesBenefit();
            IBenefitDescriber benefitDescriber = new ExtraCaloriesBenefitDescriber(benefit);
            ExtraCaloriesTooltipLibrary.BenefitDescriber = benefitDescriber;
            var ecopediaGenerator = new ExtraCaloriesEcopediaGenerator(benefit, benefitDescriber);
            XPBenefitsPlugin.RegisterBenefit(benefit);
            XPBenefitsEcopediaManager.Obj.RegisterEcopediaPageGenerator(benefit, ecopediaGenerator);
            UserManager.OnUserLoggedIn.Add(user => { if (benefit.Enabled) benefit.ApplyBenefitToUser(user); });
            UserManager.OnUserLoggedOut.Add(user => { if (benefit.Enabled) benefit.RemoveBenefitFromUser(user); });
        }
    }
    public partial class ExtraCaloriesBenefit : BenefitBase
    {
        protected virtual SkillRateBasedStatModifiersRegister ModifiersRegister { get; } = new SkillRateBasedStatModifiersRegister();
        
        public override void Initialize()
        {
            XPConfig xpConfig = XPBenefitsPlugin.Obj.Config;
            bool enabled = xpConfig.ExtraCaloriesEnabled;
            BenefitValue maxBenefitValue = xpConfig.ExtraCaloriesMaxBenefitValue;
            bool xpLimitEnabled = xpConfig.ExtraCaloriesXPLimitEnabled;
            string benefitFunctionType = xpConfig.ExtraCaloriesBenefitFunctionType;
            Initialize(enabled, xpConfig, maxBenefitValue, xpLimitEnabled, benefitFunctionType);
        }

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
        public ExtraCaloriesEcopediaGenerator(BenefitBase benefit, IBenefitDescriber benefitDescriber) : base(benefit, benefitDescriber)
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
    }
    public class ExtraCaloriesBenefitDescriber : IBenefitDescriber
    {
        public ExtraCaloriesBenefitDescriber(ExtraCaloriesBenefit benefit)
        {
            Benefit = benefit;
        }
        ExtraCaloriesBenefit Benefit { get; }
        IBenefitFunction BenefitFunction => Benefit.BenefitFunction;
        public LocString MaximumBenefit(User user)
        {
            float maxBenefit = Benefit.MaxBenefitValue.GetValue(user);
            return TextLoc.StyledNumLoc(maxBenefit, maxBenefit.ToString("+0;-0"));
        }
        public LocString CurrentBenefit(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return TextLoc.StyledNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"));
        }
        public LocString CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("+0;-0"), new Eco.Shared.Math.Range(0, Benefit.MaxBenefitValue.GetValue(user)));
        }
        public LocString CurrentInput(User user) => BenefitFunction.Describer.CurrentInput(user);
        public LocString InputName(User user) => BenefitFunction.Describer.InputName(user);
        public LocString MaximumInput(User user) => BenefitFunction.Describer.MaximumInput(user);
        public LocString MeansOfImprovingStat(User user) => BenefitFunction.Describer.MeansOfImprovingStat(user);
    }
    public partial class XPConfig
    {
        private string extraCaloriesBenefitFunction = "GeometricMeanFoodHousing";

        [Category("Benefit - Extra Calories"), LocDisplayName("Enabled"), LocDescription("Disable if you don't want XP to grant extra calorie capacity. Requires restart.")]
        public bool ExtraCaloriesEnabled { get; set; } = true;

        [Category("Benefit - Extra Calories"), LocDisplayName("Max Extra Calories"), LocDescription("How much extra calorie space can be earned. " +
            "If a player exceeds the 'maximum' XP it will be higher unless the XP limit is enabled. Requires restart.")]
        public float ExtraCaloriesMaxBenefitValue { get; set; } = 6000;

        [Category("Benefit - Extra Calories"), LocDisplayName("Limit XP"), LocDescription(XPConfigServerDescriptions.XPLimitDescription)]
        public bool ExtraCaloriesXPLimitEnabled { get; set; } = false;

        [Category("Benefit - Extra Calories"), LocDisplayName("Benefit Function"), LocDescription(XPConfigServerDescriptions.BenefitFunctionTypeDescription)]
        public string ExtraCaloriesBenefitFunctionType { get => XPBenefitsPlugin.Obj.ValidateBenefitFunctionType(extraCaloriesBenefitFunction); set { extraCaloriesBenefitFunction = value; } }
    }
    [TooltipLibrary]
    public static class ExtraCaloriesTooltipLibrary
    {
        public static IBenefitDescriber BenefitDescriber { get; set; }
        [NewTooltip(Eco.Shared.Items.CacheAs.Disabled, 103, overrideType: typeof(Stomach))]
        public static LocString ExtraCaloriesStomachTooltip(this Stomach stomach)
        {
            ExtraCaloriesBenefit benefit = XPBenefitsPlugin.Obj.GetBenefit<ExtraCaloriesBenefit>();
            if (benefit == null || !benefit.Enabled)
            {
                return LocString.Empty;
            }
            User user = stomach.Owner;
            var ecopediaGenerator = XPBenefitsEcopediaManager.Obj.GetEcopedia(benefit);
            LocString extraWeightLimit = BenefitDescriber.CurrentBenefit(user);
            return new TooltipSection(Localizer.Do($"Calorie limit boosted by {extraWeightLimit} due to {ecopediaGenerator.GetPageLink()}."));
        }
    }
}
