using Eco.Gameplay.Players;

namespace Ecompatible
{
    public interface IValueModificationContext
    {
    }
    public interface IValueModificationUserContext : IValueModificationContext
    {
        User User { get; }
    }
}
