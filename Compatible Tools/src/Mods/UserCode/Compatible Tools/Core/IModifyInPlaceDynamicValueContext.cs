using Eco.Gameplay.Players;

namespace EcompatibleTools
{
    public interface IModifyValueInPlaceContext
    {
        User User { get; }
        float FloatValue { get; set; }
        int IntValue { get; set; }
    }
}
