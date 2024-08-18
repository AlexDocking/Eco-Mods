using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        private IResolvedSequenceDescriber<float> TableRoundDown { get; } = new ResolvedIntFromFloatTableDescriber();
        public LocString DescribeSequenceAsTableAndRoundDown(IResolvedSequence<float> resolvedSequence)
        {
            return TableRoundDown.DescribeSequence(resolvedSequence);
        }
    }
}
