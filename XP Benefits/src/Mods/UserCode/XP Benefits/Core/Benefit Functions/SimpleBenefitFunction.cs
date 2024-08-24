using Eco.Gameplay.Players;

namespace XPBenefits
{
    public class SimpleBenefitFunction : IBenefitFunction
    {
        public IBenefitFunctionInput Input { get; }
        public BenefitValue MaximumBenefit { get; }
        public bool ClampBenefit { get; }

        public SimpleBenefitFunction(IBenefitFunctionInput input, BenefitValue maximumBenefit)
        {
            Input = input;
            MaximumBenefit = maximumBenefit;
            Describer = new InputDescriber(Input);
        }

        public IBenefitInputDescriber Describer { get; set; }

        public float CalculateBenefit(User user)
        {
            var range = Input.GetInputRange(user);
            float benefit = (Input.GetInput(user) - range.Min) / (range.Max - range.Min);
            return benefit * MaximumBenefit.GetValue(user);
        }
    }
}