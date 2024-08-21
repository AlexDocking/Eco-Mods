using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
