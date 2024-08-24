using Eco.Gameplay.DynamicValues;
using Eco.Gameplay.Players;
using System;

namespace XPBenefits
{
    public class BenefitDynamicValue : IDynamicValue
    {
        public float GetBaseValue => 0;
        public ref int ControllerID => ref id;
        private int id;
        public IBenefitFunction BenefitFunction { get; }

        public BenefitDynamicValue(IBenefitFunction benefitFunction)
        {
            BenefitFunction = benefitFunction ?? throw new ArgumentNullException(nameof(benefitFunction));
        }

        public float GetCurrentValue(IDynamicValueContext context, object obj)
        {
            User user = context.User;
            return BenefitFunction.CalculateBenefit(user);
        }

        public int GetCurrentValueInt(IDynamicValueContext context, object obj, float multiplier)
        {
            return (int)(GetCurrentValue(context, obj) * multiplier);
        }
    }
}