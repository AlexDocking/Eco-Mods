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
using Eco.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    /// <summary>
    /// Scale the benefit by the geometric mean of the scaled inputs (inputs scaled to its range).
    /// This requires all scaled inputs to be greater than zero to give any benefit.
    /// </summary>
    public class GeometricMeanBenefitFunction : IBenefitFunction
    {
        private List<IBenefitFunctionInput> Inputs { get; }
        public IBenefitInputDescriber Describer { get; }
        public BenefitValue MaximumBenefit { get; set; }

        public GeometricMeanBenefitFunction(IEnumerable<IBenefitFunctionInput> inputs, IBenefitInputDescriber describer, BenefitValue maximumBenefit)
        {
            Inputs = inputs.ToList();
            Describer = describer;
            MaximumBenefit = maximumBenefit;
        }

        public float CalculateBenefit(User user)
        {
            float product = Inputs.Select(input => input.GetScaledInput(user)).Mult();
            float fractionOfBenefitToApply = (float)Math.Pow(product, 1f / Inputs.Count);
            return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
        }
    }
}