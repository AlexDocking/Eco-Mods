// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;

namespace Ecompatible
{
    public class EcompatibleInventoryPlugin : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "";

        public string GetStatus() => "";
        public void Initialize(TimedTask timer)
        {
            foreach(User user in UserManager.Users) AddCarriedInventoryRestriction(user);
            UserManager.NewUserJoinedEvent.Add(AddCarriedInventoryRestriction);
        }
        private void AddCarriedInventoryRestriction(User user)
        {
            user.Inventory.Carried.AddInvRestriction(new ResolvedStackSizeInventoryRestriction(user, user.Inventory.Carried, ValueResolvers.Inventory.User.Carried));
        }
    }
    internal class ResolvedStackSizeInventoryRestriction : InventoryRestriction
    {
        public override LocString Message => LocString.Empty;
        public override bool SurpassStackSize => true;

        private Inventory Inventory { get; }
        private User Owner { get; }
        private IValueResolver<float, IUserPutItemInInventoryContext> StackSizeResolver { get; }

        public ResolvedStackSizeInventoryRestriction(User owner, Inventory inventory, IValueResolver<float, IUserPutItemInInventoryContext> stackSizeResolver)
        {
            Owner = owner;
            Inventory = inventory;
            StackSizeResolver = stackSizeResolver;
        }

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            IUserPutItemInInventoryContext context = ContextFactory.CreateUserPutItemInInventoryContext(
                user:Owner,
                inventory:Inventory,
                itemToPutInInventory:item);
            int maxStackSize = StackSizeResolver.ResolveInt(0, context);
            return maxStackSize;
        }
    }
    internal class UniqueItemStackSizeModifier<TContext> : IValueModifier<float, TContext> where TContext : IUserPutItemInInventoryContext
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float, TContext> functionInput)
        {
            var context = functionInput.Context;
            if (context.ItemToPutInInventory is not Item item) return null;
            if (Item.IsRestrictedToSingleItem(item.Type)) return OutputFactory.BaseLevel(1, Localizer.DoStr("Base Level (unique item)"));
            return null;
        }
    }
}
