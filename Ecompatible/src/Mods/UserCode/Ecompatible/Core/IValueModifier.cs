using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace Ecompatible
{
    public interface IValueModifier<T, TContext> where TContext : IContext
    {
        IModificationOutput<T> ModifyValue(IModificationInput<T, TContext> functionInput);
    }
}
