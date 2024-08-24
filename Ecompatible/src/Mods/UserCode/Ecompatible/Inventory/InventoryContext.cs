// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Gameplay.Items;
using Eco.Gameplay.Players;

namespace Ecompatible
{
    public static partial class ContextFactory
    {
        public static IUserPutItemInInventoryContext CreateUserPutItemInInventoryContext(User user, Inventory inventory, Item itemToPutInInventory)
        {
            return new UserPutItemInInventoryContext(user, inventory, itemToPutInInventory);
        }
        public static IUserPutItemInInventoryContext CreateUserPutItemInInventoryContext(User user, Item itemToPutInInventory)
        {
            return CreateUserPutItemInInventoryContext(user, user.Inventory, itemToPutInInventory);
        }
    }
    public interface IPutItemInInventoryContext : IContext
    {
        Inventory Inventory { get; }
        Item ItemToPutInInventory { get; }
    }
    public interface IUserPutItemInInventoryContext : IPutItemInInventoryContext, IUserContext
    {
    }
    internal class UserPutItemInInventoryContext : IUserPutItemInInventoryContext
    {
        public UserPutItemInInventoryContext(User user, Inventory inventory, Item itemToPutInInventory)
        {
            User = user;
            Inventory = inventory;
            ItemToPutInInventory = itemToPutInInventory;
        }

        public User User { get; set; }

        public Inventory Inventory { get; set; }

        public Item ItemToPutInInventory { get; set; }
    }
}
