using Eco.Gameplay.Players;
using Eco.Shared.Math;

namespace XPBenefits
{
    public interface IBenefitFunctionInput
    {
        float GetInput(User user);
        Range GetInputRange(User user);
    }
    public static class BenefitFunctionInputExtensions
    {
        public static float GetNormalizedInput(this IBenefitFunctionInput input, User user)
        {
            Range range = input.GetInputRange(user);
            float val = input.GetInput(user);
            return (val - range.Min) / (range.Max - range.Min);
        }
    }
}
