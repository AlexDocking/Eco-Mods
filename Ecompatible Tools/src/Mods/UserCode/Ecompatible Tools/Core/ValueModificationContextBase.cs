using Eco.Gameplay.Players;

namespace Ecompatible
{
    public class ValueModificationContextBase : IValueModificationContext
    {
        public User User { get; init; }
        public float FloatValue { get; set; }
    }
}
