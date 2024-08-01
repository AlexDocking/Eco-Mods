using Eco.Shared.Localization;

namespace Ecompatible
{
    public interface IValueModifier
    {
        void ModifyValue(IValueModificationContext context, out LocString description);
    }
}
