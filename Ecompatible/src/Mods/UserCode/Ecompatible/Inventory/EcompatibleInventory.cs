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
    public static partial class ValueResolvers
    {
        public static InventoryResolvers Inventory { get; } = new InventoryResolvers();
    }
    public partial class InventoryResolvers
    {
        public UserInventoryResolvers User { get; } = new UserInventoryResolvers();
    }
    public partial class UserInventoryResolvers
    {
        public IPriorityValueResolver<float, IUserPutItemInInventoryContext> Carried { get; } = ValueResolverFactory.CreatePriorityResolver<float, IUserPutItemInInventoryContext>(
            (float.MinValue, new MaxStackSizePickupLimit<IUserPutItemInInventoryContext>()),
            (float.MaxValue, new UniqueItemStackSizeModifier<IUserPutItemInInventoryContext>()));
    }
}
