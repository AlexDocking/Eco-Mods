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
using Eco.Core.Plugins.Interfaces;
using Eco.Gameplay.EcopediaRoot;
using Eco.Gameplay.Players;
using Eco.Gameplay.Systems.TextLinks;
using Eco.Shared.Localization;
using Eco.Shared.Math;
using Eco.Shared.Utils;
using System.Collections.Generic;
using System.Linq;

namespace XPBenefits
{
    public class RegisterSkillRateBenefitFunction : IModInit
    {
        public static void Initialize()
        {
            XPBenefitsPlugin.RegisterBenefitFunctionFactory(new SkillRateBenefitFunctionFactory());
        }
    }
    /// <summary>
    /// Scale the benefit by the amount of food and housing xp the player has,
    /// treating both sources of xp equally
    /// </summary>
    public class SkillRateBenefitFunction : IBenefitFunction, IBenefitInputDescriber
    {
        private List<IBenefitFunctionInput> Inputs { get; }
        public BenefitValue MaximumBenefit { get; set; }
        public SkillRateBenefitFunction(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            MaximumBenefit = maximumBenefit;

            FoodXPInput foodInput = new FoodXPInput(xpConfig);
            HousingXPInput housingInput = new HousingXPInput(xpConfig);
            List<IBenefitFunctionInput> inputs = new List<IBenefitFunctionInput>()
            {
                foodInput,
                housingInput
            };
            inputs = inputs.Select(input => new ClampInput(input, xpLimitEnabled)).Cast<IBenefitFunctionInput>().ToList();
            Inputs = inputs;
        }
        public float CalculateBenefit(User user)
        {
            float fractionOfBenefitToApply = Inputs.Select(input => input.GetScaledInput(user)).AverageOrDefault();
            return fractionOfBenefitToApply * MaximumBenefit.GetValue(user);
        }
        private string ExperienceEcopediaPageLink => Ecopedia.Obj.GetPage("Experience").UILink(Localizer.DoStr("XP Multiplier"));
        #region IBenefitInputDescriber
        public IBenefitInputDescriber Describer => this;
        public LocString InputName(User user) => Localizer.Do($"{ExperienceEcopediaPageLink}");
        public LocString MeansOfImprovingStat(User user) => Localizer.Do($"You can increase this benefit by improving your {ExperienceEcopediaPageLink}");
        public LocString MaximumInput(User user)
        {
            float maxXPMultiplier = Inputs.Select(input => input.GetInputRange(user).Max).Sum();
            return Localizer.Do($"an XP multiplier of {TextLoc.StyledNumLoc(maxXPMultiplier, maxXPMultiplier.ToString("0.#"))}");
        }
        public LocString CurrentInput(User user)
        {
            float xpMultiplier = Inputs.Select(input => input.GetInput(user)).Sum();
            return Localizer.Do($"an XP multiplier of {DisplayUtils.GradientNumLoc(xpMultiplier, xpMultiplier.ToString("0.#"), new Range(Inputs.Select(input => input.GetInputRange(user).Min).Sum(), Inputs.Select(input => input.GetInputRange(user).Max).Sum()))}");
        }
        #endregion
    }
    public class SkillRateBenefitFunctionFactory : IBenefitFunctionFactory
    {
        public string Name { get; } = "SkillRate";
        public string Description { get; } = "Treats both food and housing xp equally after they have been scaled by their own maximums, taking their average. [100% food + 0% housing] and [0% food + 100% housing] are both equivalent to [50% food + 50% housing].";
        public IBenefitFunction Create(XPConfig xpConfig, BenefitValue maximumBenefit, bool xpLimitEnabled = false)
        {
            return new SkillRateBenefitFunction(xpConfig, maximumBenefit, xpLimitEnabled);
        }
    }
}
