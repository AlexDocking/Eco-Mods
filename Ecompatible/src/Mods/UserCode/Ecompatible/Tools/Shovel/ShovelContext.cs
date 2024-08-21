using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using System;
using System.Numerics;

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
