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

using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Serialization;

namespace XPBenefits.Tests
{
    /// <summary>
    /// Remove dependency on SLG foods which could change in future updates and break tests which rely on specific food xp values from nutrients and calories
    /// </summary>
    [Serialized]
    public class TestFood : FoodItem
    {
        public override Nutrients Nutrition => new Nutrients() { Carbs = 10, Fat = 10, Protein = 10, Vitamins = 10 };

        public override float Calories => 1000;

        protected override float BaseShelfLife => 0;
    }
}
