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

namespace XPBenefits
{
    public class SimpleBenefitFunction : IBenefitFunction
    {
        public IBenefitFunctionInput Input { get; }
        public BenefitValue MaximumBenefit { get; }
        public bool ClampBenefit { get; }

        public SimpleBenefitFunction(IBenefitFunctionInput input, BenefitValue maximumBenefit, bool clampBenefit)
        {
            Input = input;
            MaximumBenefit = maximumBenefit;
            ClampBenefit = clampBenefit;
            Describer = new InputDescriber(Input, this, maximumBenefit);
        }

        public IBenefitDescriber Describer { get; set; }

        public float CalculateBenefit(User user)
        {
            var range = Input.GetInputRange(user);
            float benefit = (Input.GetInput(user) - range.Min) / (range.Max - range.Min);
            benefit = ClampBenefit ? Math.Clamp(benefit, 0, 1) : benefit;
            return benefit * MaximumBenefit.GetValue(user);
        }
    }
}
