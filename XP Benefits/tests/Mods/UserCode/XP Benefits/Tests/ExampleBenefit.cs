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
using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits.Tests
{
    public class RegisterExampleBenefit : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "Mods";
        public string GetStatus() => "";
        public void Initialize(TimedTask timer)
        {
            var benefit = new ExampleBenefit();
            IBenefitDescriber benefitDescriber = new ExampleBenefitDescriber(benefit);
            ExtraCaloriesTooltipLibrary.BenefitDescriber = benefitDescriber;
            var ecopediaGenerator = new ExampleBenefitEcopediaGenerator(benefit, benefitDescriber);
            XPBenefitsPlugin.RegisterBenefit(benefit);
            XPBenefitsEcopediaManager.Obj.RegisterEcopediaPageGenerator(benefit, ecopediaGenerator);
        }
    }
    public class ExampleBenefitDescriber : IBenefitDescriber
    {
        public ExampleBenefitDescriber(ExampleBenefit benefit)
        {
            Benefit = benefit;
        }
        public ExampleBenefit Benefit { get; }
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
    public class ExampleBenefit : BenefitBase
    {
        public override void ApplyBenefitToUser(User user)
        {
        }

        public override void Initialize()
        {
        }

        public override void RemoveBenefitFromUser(User user)
        {
        }
    }
    public class ExampleBenefitEcopediaGenerator : BenefitEcopediaGenerator
    {
        public ExampleBenefitEcopediaGenerator(BenefitBase benefit, IBenefitDescriber benefitDescriber) : base(benefit, benefitDescriber)
        {
        }

        public override string Summary => "Example Summary";
        public override LocString DisplayName => Localizer.DoStr("Example Benefit");
        public override string PageName => "Example Benefit";
        public override float PagePriority => 4;
        public override LocString BenefitDescription => LocString.Empty;
    }
}
