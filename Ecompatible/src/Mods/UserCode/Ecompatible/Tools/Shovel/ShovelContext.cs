using Eco.Gameplay.Items;
// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.


using Eco.Gameplay.Players;
using Eco.Mods.TechTree;

namespace Ecompatible
{
    public static partial class ContextFactory
    {
        public static IShovelPickUpContext CreateShovelPickUpContext(User user, ShovelItem shovel, Item itemToPutInInventory)
        {
            return new ShovelPickUpContext(user, shovel, user.Inventory, itemToPutInInventory);
        }
    }
    public interface IUserContext : IContext
    {
        User User { get; }
    }
    public interface IShovelPickUpContext : IUserPutItemInInventoryContext
    {
        ShovelItem Shovel { get; }
    }
    internal class ShovelPickUpContext : IShovelPickUpContext
    {
        public ShovelPickUpContext(User user, ShovelItem shovel, Inventory inventory, Item itemToPutInInventory)
        {
            User = user;
            Shovel = shovel;
            Inventory = inventory;
            ItemToPutInInventory = itemToPutInInventory;
        }

        public User User { get; set; }
        public ShovelItem Shovel { get; set; }
        public Inventory Inventory { get; set; }
        public Item ItemToPutInInventory { get; set; }
    }
}
