using Eco.Gameplay.Players;

namespace XPBenefits
{
    public class BenefitValue
    {
        protected float Value { get; set; }
        public virtual float GetValue(User user)
        {
            return Value;
        }
        public BenefitValue(float value)
        {
            Value = value;
        }
        public static implicit operator BenefitValue(float value)
        {
            return new BenefitValue(value);
        }
    }
}