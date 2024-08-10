using Eco.Shared.Localization;
using EcompatibleTools;

namespace Ecompatible
{
    public interface IValueModifier
    {
        void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails);
    }
}
