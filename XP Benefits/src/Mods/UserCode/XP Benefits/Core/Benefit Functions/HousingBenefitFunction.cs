using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using System;

namespace XPBenefits
{
    public class RegisterHousingBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new HousingBenefitFunctionFactory());
        }
    }

    public class HousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "HousingOnly";
        public string Description { get; } = "Uses only the amount of housing xp the player has.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            HousingXPInput input = new HousingXPInput(xpConfig);
            SimpleBenefitFunction benefitFunction = new SimpleBenefitFunction(new ClampInput(input, xpLimitEnabled), maximumBenefit);
            LocString inputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"))} multiplier");
            benefitFunction.Describer = new InputDescriber(input)
            {
                InputName = "housing XP",
                InputTitle = inputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {inputTitle}"),
            };
            return benefitFunction;
        }
    }
}