using Eco.Gameplay.Players;
using Eco.Shared.Math;

namespace XPBenefits
{
    public class HousingXPInput : IBenefitFunctionInput
    {
        public HousingXPInput(XPConfig xpConfig)
        {
            XPConfig = xpConfig ?? throw new System.ArgumentNullException(nameof(xpConfig));
        }

        private XPConfig XPConfig { get; }

        public float GetInput(User user) => SkillRateUtil.HousingXP(user);

        public Range GetInputRange(User user) => new Range(0, XPConfig.MaximumHousingXP);
    }
}