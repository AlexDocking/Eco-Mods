using Eco.Gameplay.Players;
using System;
using Range = Eco.Shared.Math.Range;

namespace XPBenefits
{
    public class ClampInput : IBenefitFunctionInput
    {
        public ClampInput(IBenefitFunctionInput input, bool clamp)
        {
            Input = input;
            Clamp = clamp;
        }

        public IBenefitFunctionInput Input { get; }
        public bool Clamp { get; set; }

        public float GetInput(User user)
        {
            if (!Clamp) return Input.GetInput(user);
            Range range = Input.GetInputRange(user);
            return Math.Clamp(Input.GetInput(user), range.Min, range.Max);
        }

        public Range GetInputRange(User user) => Input.GetInputRange(user);
    }
}