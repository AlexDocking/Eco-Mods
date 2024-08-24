using Eco.Gameplay.Players;

namespace XPBenefits
{
    public interface IBenefitFunction
    {
        public float CalculateBenefit(User user);
        IBenefitInputDescriber Describer { get; }
    }
}