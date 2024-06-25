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
        public BenefitDescriber(IBenefitInputDescriber inputDescriber, IBenefitOutputDescriber outputDescriber)
        {
            InputDescriber = inputDescriber;
            OutputDescriber = outputDescriber;
        }

        public IBenefitInputDescriber InputDescriber { get; }
        public IBenefitOutputDescriber OutputDescriber { get; }

        public LocString CurrentBenefit(User user)
        {
            return OutputDescriber.CurrentBenefit(user);
        }

        public LocString CurrentBenefitEcopedia(User user)
        {
            return OutputDescriber.CurrentBenefitEcopedia(user);
        }

        public LocString CurrentInput(User user) => InputDescriber.CurrentInput(user);

        public LocString InputName(User user) => InputDescriber.InputName(user);

        public LocString MaximumBenefit(User user)
        {
            return OutputDescriber.MaximumBenefit(user);
        }

        public LocString MaximumInput(User user) => InputDescriber.MaximumInput(user);

        public LocString MeansOfImprovingStat(User user) => InputDescriber.MeansOfImprovingStat(user);
    }
}