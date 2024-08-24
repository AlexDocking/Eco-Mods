using Eco.Gameplay.Players;
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    /// <summary>
    /// Scale the benefit by the geometric mean of the scaled inputs (inputs scaled to its range).
    /// This requires all scaled inputs to be greater than zero to give any benefit.
    /// </summary>
    public class GeometricMeanBenefitFunction : IBenefitFunction
    {
        private List<IBenefitFunctionInput> Inputs { get; }
        public IBenefitInputDescriber Describer { get; }
        public BenefitValue MaximumBenefit { get; set; }

        public GeometricMeanBenefitFunction(IEnumerable<IBenefitFunctionInput> inputs, IBenefitInputDescriber describer, BenefitValue maximumBenefit)
        {
            Inputs = inputs.ToList();
            Describer = describer;
            MaximumBenefit = maximumBenefit;
        }

        public float CalculateBenefit(User user)
        {
            float product = Inputs.Select(input => input.GetNormalizedInput(user)).Mult();
            float fractionOfBenefitToApply = (float)Math.Pow(product, 1f / Inputs.Count);
            return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
        }
    }
}