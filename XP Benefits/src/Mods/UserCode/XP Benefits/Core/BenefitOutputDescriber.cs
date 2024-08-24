using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class BenefitOutputDescriber : IBenefitOutputDescriber
    {
        
        public BenefitOutputDescriber(IBenefitFunction benefitFunction, BenefitValue maximumBenefit)
        {
            BenefitFunction = benefitFunction;
            MaximumBenefit = maximumBenefit;
        }
        
        public IBenefitFunction BenefitFunction { get; }

        public BenefitValue MaximumBenefit { get; }
        
        LocString IBenefitOutputDescriber.MaximumBenefit(User user) => TextLoc.StyledNum(MaximumBenefit.GetValue(user));
        
        LocString IBenefitOutputDescriber.CurrentBenefit(User user) => TextLoc.StyledNum(BenefitFunction.CalculateBenefit(user));

        LocString IBenefitOutputDescriber.CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("0.#"), new Eco.Shared.Math.Range(0, MaximumBenefit.GetValue(user)));
        }

    }
}