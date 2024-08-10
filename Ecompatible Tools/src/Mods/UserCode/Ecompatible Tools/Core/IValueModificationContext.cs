using Eco.Gameplay.Players;

namespace Ecompatible
{
    public interface IValueModificationContext
    {
        User User { get; }
        float FloatValue { get; set; }
    }
}
