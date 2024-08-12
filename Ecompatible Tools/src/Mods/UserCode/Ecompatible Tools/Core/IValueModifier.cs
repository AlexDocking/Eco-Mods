using Eco.Shared.Localization;

namespace Ecompatible
{
    public interface IValueModifier
    {
        IModificationOutput ModifyValue(IModificationInput functionInput);
    }
}
