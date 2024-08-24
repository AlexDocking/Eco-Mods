// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Utils;
using EcoTestTools;
using System;
using System.Linq;

namespace Ecompatible.Tests
{
    [ChatCommandHandler]
    internal class TestShovel
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestEcompatibleShovel()
        {
            Test.Run(ShouldDetermineDiggableTypes);
        }

        private static void ShouldDetermineDiggableTypes()
        {
            ShovelItem[] shovels = typeof(ShovelItem).ConcreteTypes().Select(type => (ShovelItem)Item.Get(type)).ToArray();
            Type[] diggableItemTypes = new Type[] { typeof(DirtItem), typeof(CrushedSandstoneItem) };
            Type[] nonDiggableItemTypes = new Type[] { typeof(FlatSteelItem), typeof(WorkbenchItem) };
            foreach (ShovelItem shovel in shovels)
            {
                foreach (Type itemType in diggableItemTypes)
                {
                    Assert.IsTrue(shovel.IsDiggable(itemType));
                }
                foreach(Type itemType in nonDiggableItemTypes)
                {
                    Assert.IsFalse(shovel.IsDiggable(itemType));
                }
            }
        }
    }
}
