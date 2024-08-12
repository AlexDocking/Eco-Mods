using Eco.Mods.Organisms;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public class TreeEntityMaxPickUpModificationContext : ValueModificationContextBase
    {
        public TreeEntity Tree { get; init; }
        public AxeItem Axe { get; init; }
    }
}
