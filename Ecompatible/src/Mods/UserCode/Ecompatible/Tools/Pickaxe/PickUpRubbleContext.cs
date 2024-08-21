using Eco.Gameplay.Items;
using Eco.Gameplay.Objects;
using Eco.Gameplay.Players;
using Eco.Mods.TechTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
