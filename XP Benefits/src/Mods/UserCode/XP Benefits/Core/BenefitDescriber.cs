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

namespace XPBenefits
{
    public class BenefitDescriber : IBenefitDescriber
    {
        public BenefitDescriber(IBenefitInputDescriber inputDescriber, IBenefitFunction benefitFunction, BenefitValue maximumBenefit)
        {
            InputDescriber = inputDescriber;
            BenefitFunction = benefitFunction;
            MaximumBenefit = maximumBenefit;
        }

        public IBenefitInputDescriber InputDescriber { get; }
        public IBenefitFunction BenefitFunction { get; }
        public BenefitValue MaximumBenefit { get; }
        
        LocString IBenefitDescriber.MaximumBenefit(User user) => TextLoc.StyledNum(MaximumBenefit.GetValue(user));
        
        LocString IBenefitDescriber.CurrentBenefit(User user) => TextLoc.StyledNum(BenefitFunction.CalculateBenefit(user));

        LocString IBenefitDescriber.CurrentBenefitEcopedia(User user)
        {
            float currentBenefit = BenefitFunction.CalculateBenefit(user);
            return DisplayUtils.GradientNumLoc(currentBenefit, currentBenefit.ToString("0.#"), new Eco.Shared.Math.Range(0, MaximumBenefit.GetValue(user)));
        }

        public LocString CurrentInput(User user) => InputDescriber.CurrentInput(user);

        public LocString InputName(User user) => InputDescriber.InputName(user);

        public LocString MaximumInput(User user) => InputDescriber.MaximumInput(user);

        public LocString MeansOfImprovingStat(User user) => InputDescriber.MeansOfImprovingStat(user);
    }
}