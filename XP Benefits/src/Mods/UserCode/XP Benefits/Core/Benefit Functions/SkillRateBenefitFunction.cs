using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    public class RegisterSkillRateBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new SkillRateBenefitFunctionFactory());
        }
    }

    public class SkillRateBenefitDescriber : IBenefitInputDescriber
    {
        public SkillRateBenefitDescriber(MeanOfNormalizedInputsBenefitFunction benefitFunction)
        {
            BenefitFunction = benefitFunction;
        }

        public MeanOfNormalizedInputsBenefitFunction BenefitFunction { get; }
        private static LocString ExperienceEcopediaPageLink() => Ecopedia.Obj.UILinkPageWithContent("Experience", Localizer.DoStr("XP Multiplier"));
        private IEnumerable<IBenefitFunctionInput> Inputs => BenefitFunction.Inputs;

        public LocString CurrentInput(User user)
        {
            float xpMultiplier = Inputs.Select(input => input.GetInput(user)).Sum();
            return Localizer.Do($"an XP multiplier of {DisplayUtils.GradientNumLoc(xpMultiplier, xpMultiplier.ToString("0.#"), new Range(Inputs.Select(input => input.GetInputRange(user).Min).Sum(), Inputs.Select(input => input.GetInputRange(user).Max).Sum()))}");
        }

        public LocString InputName(User user) => ExperienceEcopediaPageLink();

        public LocString MaximumInput(User user)
        {
            float maxXPMultiplier = Inputs.Select(input => input.GetInputRange(user).Max).Sum();
            return Localizer.Do($"an XP multiplier of {TextLoc.StyledNumLoc(maxXPMultiplier, maxXPMultiplier.ToString("0.#"))}");
        }

        public LocString MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {ExperienceEcopediaPageLink()}");
    }

    public class SkillRateBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Description { get; } = "Treats both food and housing xp equally after they have been scaled by their own maximums, taking their average. [100% food + 0% housing] and [0% food + 100% housing] are both equivalent to [50% food + 50% housing].";
        public string Name { get; } = "SkillRate";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            FoodXPInput foodInput = new FoodXPInput(xpConfig);
            HousingXPInput housingInput = new HousingXPInput(xpConfig);
            List<IBenefitFunctionInput> inputs = new List<IBenefitFunctionInput>()
            {
                foodInput,
                housingInput
            };
            MeanOfNormalizedInputsBenefitFunction benefitFunction = new MeanOfNormalizedInputsBenefitFunction(inputs, maximumBenefit, xpLimitEnabled);
            benefitFunction.Describer = new SkillRateBenefitDescriber(benefitFunction);
            return benefitFunction;
        }
    }
}