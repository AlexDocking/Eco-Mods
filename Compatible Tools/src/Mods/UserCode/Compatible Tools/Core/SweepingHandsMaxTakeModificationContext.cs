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
namespace CompatibleTools
{
    using Eco.Gameplay.Items;
    using Eco.Gameplay.Players;
    using Eco.Mods.TechTree;
    using System.Numerics;

    public class SweepingHandsMaxTakeModificationContext : IModifyValueInPlaceContext
    {
        public User User { get; init; }
        public Item Resource { get; init; }
        public MiningSweepingHandsTalent SweepingHandsTalent { get; init; }
        public Vector3 Position => User.Position;
        public float FloatValue { get; set; }
        public int IntValue { get; set; }
    }
}
