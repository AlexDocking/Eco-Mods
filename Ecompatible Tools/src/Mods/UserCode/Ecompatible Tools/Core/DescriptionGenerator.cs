using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        private IResolvedSequenceDescriber<float> TableRoundDown { get; } = new ResolvedIntFromFloatTableDescriber();
        public LocString DescribeSequenceAsTableAndRoundDown(ResolvedSequence<float> resolvedSequence)
        {
            return TableRoundDown.DescribeSequence(resolvedSequence);
        }
    }
}
