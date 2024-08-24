using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

namespace Ecompatible
{
    public static partial class ContextFactory
    {
        public static IUserPickUpRubbleContext CreatePickUpRubbleContext(User user, RubbleObject rubble)
        {
            return new PickUpRubbleContext(user, rubble, user.Inventory);
        }
    }
    public interface IUserPickUpRubbleContext : IUserPutItemInInventoryContext
    {
        RubbleObject Rubble { get; }
    }
    internal class PickUpRubbleContext : IUserPickUpRubbleContext
    {
        public PickUpRubbleContext(User user, RubbleObject rubble, Inventory inventory)
        {
            User = user;
            Rubble = rubble;
            Inventory = inventory;
            ItemToPutInInventory = (rubble is IRepresentsItem representsItem) ? Item.Get(representsItem.RepresentedItemType) : null;
        }
        
        public User User { get; set; }

        public RubbleObject Rubble { get; set; }

        public Inventory Inventory { get; set; }

        public Item ItemToPutInInventory { get; set; }

    }
}
