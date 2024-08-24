using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace XPBenefits
{
    public class BenefitDescriber : IBenefitDescriber
    {
        public BenefitDescriber(IBenefitInputDescriber inputDescriber, IBenefitOutputDescriber outputDescriber)
        {
            InputDescriber = inputDescriber;
            OutputDescriber = outputDescriber;
        }

        public IBenefitInputDescriber InputDescriber { get; }
        public IBenefitOutputDescriber OutputDescriber { get; }

        public LocString CurrentBenefit(User user)
        {
            return OutputDescriber.CurrentBenefit(user);
        }

        public LocString CurrentBenefitEcopedia(User user)
        {
            return OutputDescriber.CurrentBenefitEcopedia(user);
        }

        public LocString CurrentInput(User user) => InputDescriber.CurrentInput(user);

        public LocString InputName(User user) => InputDescriber.InputName(user);

        public LocString MaximumBenefit(User user)
        {
            return OutputDescriber.MaximumBenefit(user);
        }

        public LocString MaximumInput(User user) => InputDescriber.MaximumInput(user);

        public LocString MeansOfImprovingStat(User user) => InputDescriber.MeansOfImprovingStat(user);
    }
}