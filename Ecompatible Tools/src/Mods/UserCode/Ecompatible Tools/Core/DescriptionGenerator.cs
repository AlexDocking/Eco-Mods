using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        public LocString BuildModificationListDescriptionInt(int intOutput, ResolvedSequence<float> resolvedSequence)
        {
            ResolvedValueDescriber describer = new ResolvedValueDescriber();
            return describer.GenerateDescription(intOutput, resolvedSequence);
        }
    }
}
