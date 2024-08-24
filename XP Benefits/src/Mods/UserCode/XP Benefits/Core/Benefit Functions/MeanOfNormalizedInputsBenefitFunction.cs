using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    /// <summary>
    /// Normalise each input and take the average
    /// </summary>
    public class MeanOfNormalizedInputsBenefitFunction : IBenefitFunction
    {
        public MeanOfNormalizedInputsBenefitFunction(IEnumerable<IBenefitFunctionInput> inputs, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            MaximumBenefit = maximumBenefit;

            
            inputs = inputs.Select(input => new ClampInput(input, xpLimitEnabled)).Cast<IBenefitFunctionInput>().ToList();
            Inputs = inputs;
        }

        public IBenefitInputDescriber Describer { get; set; }
        public IEnumerable<IBenefitFunctionInput> Inputs { get; }
        public BenefitValue MaximumBenefit { get; set; }

        public float CalculateBenefit(User user)
        {
            float fractionOfBenefitToApply = Inputs.Select(input => input.GetNormalizedInput(user)).AverageOrDefault();
            return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
        }
    }
}