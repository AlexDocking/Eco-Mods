using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class RegisterFoodBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new FoodBenefitFunctionFactory());
        }
    }

    /// <summary>
    /// Create a function that scales the benefit by the amount of food xp the player has
    /// </summary>
    public class FoodBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "FoodOnly";
        public string Description { get; } = "Uses only the amount of food xp the player has.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            FoodXPInput input = new FoodXPInput(xpConfig);
            SimpleBenefitFunction benefitFunction = new SimpleBenefitFunction(new ClampInput(input, xpLimitEnabled), maximumBenefit);

            static LocString GetInputTitle() => Localizer.Do($"{Ecopedia.Obj.UILinkPage("Nutrition")} multiplier");
            benefitFunction.Describer = new InputDescriber(input)
            {
                InputName = "food XP",
                InputTitleGetter = GetInputTitle,
                MeansOfImprovingStatDescriptionGetter = () => Localizer.Do($"You can increase this benefit by improving your {GetInputTitle()}"),
                AdditionalInfo = Localizer.DoStr("Note that 'Base Gain' is ignored when calculating your nutrition percentage"),
            };
            return benefitFunction;
        }
    }
}