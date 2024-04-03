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
namespace XPBenefits
{
    internal static class XPConfigServerDescriptions
    {
        public const string BenefitFunctionTypeDescription = "How the player's XP is combined to scale the reward.\n" +
                                                            "GeometricMeanFoodHousing: Uses a combination of the amount of food and housing xp the player has in such a way as to require both sources of xp to give any benefit.\n" +
                                                            "FoodOnly: Uses only the amount of food xp the player has.\n" +
                                                            "HousingOnly: Uses only the amount of housing xp the player has.\n" +
                                                            "SkillRate: Treats both food and housing xp equally after they have been scaled by their own maximums, taking their average. " +
                                                            "[100% food + 0% housing] and [0% food + 100% housing] are both equivalent to [50% food + 50% housing].\n" +
                                                            "Requires restart.";
        public const string XPLimitDescription = "Enable if you want to cap the player's XP to prevent exceeding the maximum reward. Requires restart.";
    }
}