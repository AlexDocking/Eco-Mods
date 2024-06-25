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
    public class InputDescriber : IBenefitInputDescriber
    {
        public InputDescriber(IBenefitFunctionInput input)
        {
            Input = input;
        }

        public string InputName { get; set; }
        public LocString InputTitle { get; set; }
        private IBenefitFunctionInput Input { get; }

        LocString IBenefitInputDescriber.InputName(User user) => InputTitle;

        LocString IBenefitInputDescriber.MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {InputTitle}. Note that 'Base Gain' is ignored when calculating your nutrition percentage");

        LocString IBenefitInputDescriber.MaximumInput(User user)
        {
            float max = Input.GetInputRange(user).Max;
            return Localizer.Do($"{TextLoc.StyledNumLoc(max, max.ToString("0.#"))} {InputName}");
        }

        LocString IBenefitInputDescriber.CurrentInput(User user)
        {
            float inputValue = Input.GetInput(user);
            return Localizer.Do($"{DisplayUtils.GradientNumLoc(inputValue, inputValue.ToString("0.#"), Input.GetInputRange(user))} {InputName}");
        }

    }
}