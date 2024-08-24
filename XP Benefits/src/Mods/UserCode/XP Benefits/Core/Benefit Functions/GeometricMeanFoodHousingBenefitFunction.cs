using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    public class RegisterGeometricMeanFoodHousingBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new GeometricMeanFoodHousingBenefitFunctionFactory());
        }
    }

    public class GeometricMeanFoodHousingBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name => "GeometricMeanFoodHousing";
        public string Description => "Uses a combination of the amount of food and housing xp the player has in such a way as to require both sources of xp to give any benefit.";

        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            FoodXPInput foodInput = new FoodXPInput(xpConfig);
            HousingXPInput housingInput = new HousingXPInput(xpConfig);
            List<IBenefitFunctionInput> inputs = new List<IBenefitFunctionInput>()
            {
                foodInput,
                housingInput
            };
            inputs = inputs.Select(input => new ClampInput(input, xpLimitEnabled)).Cast<IBenefitFunctionInput>().ToList();
            LocString foodInputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Nutrition").UILink()}");
            InputDescriber foodInputDescriber = new InputDescriber(foodInput)
            {
                InputName = "food XP",
                InputTitle = foodInputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {foodInputTitle}"),
                AdditionalInfo = Localizer.DoStr("Note that 'Base Gain' is ignored when calculating your nutrition percentage"),
            };
            LocString housingInputTitle = Localizer.Do($"{Ecopedia.Obj.GetPage("Housing Overview").UILink(Localizer.DoStr("Housing"))}");
            InputDescriber housingInputDescriber = new InputDescriber(housingInput)
            {
                InputName = "housing XP",
                InputTitle = housingInputTitle,
                MeansOfImprovingStatDescription = Localizer.Do($"You can increase this benefit by improving your {housingInputTitle}"),
            };
            List<InputDescriber> inputDescribers = new List<InputDescriber>()
            {
                foodInputDescriber,
                housingInputDescriber
            };

            GeometricMeanInputDescriber describer = new GeometricMeanInputDescriber(inputs, inputDescribers);
            return new GeometricMeanBenefitFunction(inputs, describer, maximumBenefit);
        }
    }
}