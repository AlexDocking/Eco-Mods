using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public interface IBenefitInputDescriber
    {
        LocString InputName(User user);

        LocString CurrentInput(User user);

        LocString MaximumInput(User user);

        LocString MeansOfImprovingStat(User user);
    }
}