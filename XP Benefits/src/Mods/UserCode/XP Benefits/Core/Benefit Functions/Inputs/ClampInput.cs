//XP Benefits
//Copyright (C) 2023 Alex Docking
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

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