using Eco.Shared.Localization;

namespace Ecompatible
{
    public interface IValueModifier
    {
        void ModifyValue(IValueModificationContext context, ref IOperationDetails modificationDetails);
    }
}
