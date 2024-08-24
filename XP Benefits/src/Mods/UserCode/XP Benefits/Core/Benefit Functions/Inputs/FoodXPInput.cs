using Eco.Gameplay.Players;
using Eco.Shared.Math;

namespace XPBenefits
{
    public class FoodXPInput : IBenefitFunctionInput
    {
        public FoodXPInput(XPConfig xpConfig)
        {
            XPConfig = xpConfig ?? throw new System.ArgumentNullException(nameof(xpConfig));
        }

        XPConfig XPConfig { get; }
        public float GetInput(User user) => SkillRateUtil.FoodXP(user);
        public Range GetInputRange(User user) => new Range(XPConfig.BaseFoodXP, XPConfig.MaximumFoodXP);
    }
}
