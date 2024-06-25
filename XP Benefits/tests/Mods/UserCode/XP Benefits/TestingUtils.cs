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
using Eco.Gameplay.Housing;
using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Gameplay.Property;
using Eco.Mods.TechTree;
using Eco.Shared.Items;
using Eco.Shared.Math;
using Eco.Shared.Voxel;
using Eco.World.Blocks;
using System;

namespace XPBenefits.Tests
{
    public static class TestingUtils
    {
        public static Type[] SingleFood => new Type[] { typeof(WildStewItem) };
        public static void ResetStomach(this User user, params Type[] foodTypes)
        {
            user.Stomach.Contents.Clear();

            foreach (Type foodType in foodTypes)
            {
                if (Item.Get(foodType) is not FoodItem foodItem) continue;
                
                user.Stomach.Eat(foodItem, out _, force: true);
            }
        }
        public static void BuildHouse(this User user)
        {
            if (user.GetResidencyHouse()?.PropertyValue.Value >= 0) return;

            var (objectWithHousingValue, BuildingSize) = BuildingSpawner.SpawnBuildingWithObject(typeof(KitchenObject), user, new Vector3i(0, 100, 0));
            Deed deed = PropertyUtils.ForceClaim(objectWithHousingValue.GetDeed(), user, BuildingSize.ToXZRect());
            deed.SetResidency(user);
        }
    }
}
