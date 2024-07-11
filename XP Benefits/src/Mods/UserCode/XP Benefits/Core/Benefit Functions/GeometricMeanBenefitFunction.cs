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
using Eco.Shared.Localization;
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
    public class GeometricMeanBenefitFunction : IBenefitFunction, IBenefitInputDescriber
    {
        public BenefitValue MaximumBenefit { get; set; }
        private List<IBenefitFunctionInput> Inputs { get; set; }
        private List<InputDescriber> InputDescribers { get; set; }

        public GeometricMeanBenefitFunction(IEnumerable<IBenefitFunctionInput> inputs, IEnumerable<InputDescriber> inputDescribers, BenefitValue maximumBenefit)
        {
            Inputs = new List<IBenefitFunctionInput>(inputs);
            InputDescribers = new List<InputDescriber>(inputDescribers);
            if (Inputs.Count != InputDescribers.Count) throw new ArgumentException($"Must be same number of describers as inputs ({Inputs.Count} inputs vs {InputDescribers.Count} describers)");
            MaximumBenefit = maximumBenefit;
        }

        public float CalculateBenefit(User user)
        {
            try
            {
                float product = Inputs.Select(input => input.GetScaledInput(user)).Mult();
                float fractionOfBenefitToApply = (float)Math.Pow(product, 1f / Inputs.Count);
                return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
            }
            catch
            {
            }
            return 0;
        }

        #region IBenefitInputDescriber

        public IBenefitInputDescriber Describer => this;

        public LocString InputName(User user) => Localizer.Do($"{InputDescribers.Select(describer => (describer as IBenefitInputDescriber).InputName(user)).CommaList()} {Localizer.PluralNoNum("multiplier")}");

        public LocString MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {InputDescribers.Select(describer => (describer as IBenefitInputDescriber).InputName(user)).CommaList()} {Localizer.PluralNoNum("multiplier")}. If you want to see the greatest improvement you should improve the lowest percentage first. {InputDescribers.Select(describer => describer.AdditionalInfo).Where(s => s.IsSet()).JoinList(". ")}");

        public LocString MaximumInput(User user)
        {
            List<LocString> locs = new List<LocString>();
            for (int i = 0; i < Inputs.Count; i++)
            {
                locs.Add(Localizer.Do($"{Text.StyledNum(Inputs[i].GetInputRange(user).Max)} {InputDescribers[i].InputName}"));
            }
            return locs.CommaList();
        }

        public LocString CurrentInput(User user)
        {
            List<LocString> locs = new List<LocString>();
            for (int i = 0; i < Inputs.Count; i++)
            {
                locs.Add(Localizer.Do($"{Text.GradientColoredPercent(Inputs[i].GetScaledInput(user))} {InputDescribers[i].InputName}"));
            }
            return locs.CommaList();
        }

        #endregion IBenefitInputDescriber
    }
}