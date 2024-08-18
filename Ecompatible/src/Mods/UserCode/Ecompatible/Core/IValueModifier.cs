namespace Ecompatible
{
    public interface IValueModifier<T>
    {
        IModificationOutput<T> ModifyValue(IModificationInput<T> functionInput);
    }
}
