using Eco.Shared.Localization;
using Eco.Shared.Utils;

namespace Ecompatible
{
    public partial class DescriptionGenerator : AutoSingleton<DescriptionGenerator>
    {
        public LocString BuildModificationListDescriptionInt(AuxillaryInfo auxillaryInfo)
        {
            ResolvedValueDescriber describer = new ResolvedValueDescriber();
            return describer.GenerateDescription(auxillaryInfo);
        }
    }
}
