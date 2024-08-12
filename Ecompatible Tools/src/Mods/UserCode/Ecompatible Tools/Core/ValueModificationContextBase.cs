using Eco.Gameplay.Players;

namespace Ecompatible
{
    public class ValueModificationContextBase : IValueModificationUserContext
    {
        public User User { get; init; }
    }
}
