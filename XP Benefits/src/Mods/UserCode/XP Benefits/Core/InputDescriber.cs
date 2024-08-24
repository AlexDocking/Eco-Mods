using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class InputDescriber : IBenefitInputDescriber
    {
        public InputDescriber(IBenefitFunctionInput input)
        {
            Input = input;
        }

        public string InputName { get; set; }
        public LocString InputTitle { get; set; }
        public LocString AdditionalInfo { get; set; }
        private IBenefitFunctionInput Input { get; }
        public LocString MeansOfImprovingStatDescription { get; set; }

        LocString IBenefitInputDescriber.InputName(User user) => InputTitle;

        LocString IBenefitInputDescriber.MeansOfImprovingStat(User user) => MeansOfImprovingStatDescription + (AdditionalInfo.IsSet() ? ". " + AdditionalInfo : LocString.Empty);

        LocString IBenefitInputDescriber.MaximumInput(User user)
        {
            float max = Input.GetInputRange(user).Max;
            return Localizer.Do($"{TextLoc.StyledNumLoc(max, max.ToString("0.#"))} {InputName}");
        }

        LocString IBenefitInputDescriber.CurrentInput(User user)
        {
            float inputValue = Input.GetInput(user);
            return Localizer.Do($"{DisplayUtils.GradientNumLoc(inputValue, inputValue.ToString("0.#"), Input.GetInputRange(user))} {InputName}");
        }

    }
}