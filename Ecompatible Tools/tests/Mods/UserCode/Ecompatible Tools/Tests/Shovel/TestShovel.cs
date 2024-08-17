using Eco.Core.Tests;
using Eco.Gameplay.Items;
using Eco.Gameplay.Systems.Messaging.Chat.Commands;
using Eco.Mods.TechTree;
using Eco.Shared.Localization;
using Eco.Shared.Logging;
using Eco.Shared.Utils;
using EcoTestTools;
using System;
using System.Linq;

namespace Ecompatible.Tests
{
    [ChatCommandHandler]
    public class TestShovel
    {
        [ChatCommand(ChatAuthorizationLevel.DevTier)]
        [CITest]
        public static void TestEcompatibleShovel()
        {
            Test.Run(ShouldDetermineDiggableTypes);
        }

        private static void ShouldDetermineDiggableTypes()
        {
            EcompatibleDig[] shovels = typeof(EcompatibleDig).ConcreteTypes().Select(type => (EcompatibleDig)Item.Get(type)).ToArray();
            Type[] diggableItemTypes = new Type[] { typeof(DirtItem), typeof(CrushedSandstoneItem) };
            Type[] nonDiggableItemTypes = new Type[] { typeof(FlatSteelItem), typeof(WorkbenchItem) };
            foreach (EcompatibleDig shovel in shovels)
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
