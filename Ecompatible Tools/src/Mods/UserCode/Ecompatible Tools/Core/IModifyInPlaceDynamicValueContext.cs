using Eco.Gameplay.Players;

namespace Ecompatible
{
    public interface IModifyValueInPlaceContext
    {
        User User { get; }
        float FloatValue { get; set; }
        int IntValue { get; set; }
    }
}
