using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public interface IBenefitOutputDescriber
    {
        LocString MaximumBenefit(User user);
        LocString CurrentBenefit(User user);
        LocString CurrentBenefitEcopedia(User user);
    }
}