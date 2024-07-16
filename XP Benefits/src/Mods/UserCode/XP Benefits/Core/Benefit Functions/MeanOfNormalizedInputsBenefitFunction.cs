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
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    /// <summary>
    /// Normalise each input and take the average
    /// </summary>
    public class MeanOfNormalizedInputsBenefitFunction : IBenefitFunction
    {
        public MeanOfNormalizedInputsBenefitFunction(IEnumerable<IBenefitFunctionInput> inputs, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            MaximumBenefit = maximumBenefit;

            
            inputs = inputs.Select(input => new ClampInput(input, xpLimitEnabled)).Cast<IBenefitFunctionInput>().ToList();
            Inputs = inputs;
        }

        public IBenefitInputDescriber Describer { get; set; }
        public IEnumerable<IBenefitFunctionInput> Inputs { get; }
        public BenefitValue MaximumBenefit { get; set; }

        public float CalculateBenefit(User user)
        {
            float fractionOfBenefitToApply = Inputs.Select(input => input.GetNormalizedInput(user)).AverageOrDefault();
            return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
        }
    }
}