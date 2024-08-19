using Eco.Core.Plugins.Interfaces;
using Eco.Core.Utils;
using Eco.Gameplay.Items;
using Eco.Gameplay.Players;
using Eco.Shared.Localization;
using System;

namespace Ecompatible
{
    public class EcompatibleInventoryPlugin : IModKitPlugin, IInitializablePlugin
    {
        public string GetCategory() => "";

        public string GetStatus() => "";
        public void Initialize(TimedTask timer)
        {
            UserManager.OnUserLoggedIn.Add(OnUserLoggedIn);

        }
        private void OnUserLoggedIn(User user)
        {
            user.Inventory.Carried.AddInvRestriction(new InventorySizeRestriction(user, user.Inventory.Carried, ValueResolvers.Inventory.User.Carried));
        }
    }
    internal class InventorySizeRestriction : InventoryRestriction
    {
        public override LocString Message => LocString.Empty;
        public override bool SurpassStackSize => true;

        private Inventory Inventory { get; }
        private User Owner { get; }
        private IValueResolver<float> StackSizeResolver { get; }

        public InventorySizeRestriction(User owner, Inventory inventory, IValueResolver<float> stackSizeResolver)
        {
            Owner = owner;
            Inventory = inventory;
            StackSizeResolver = stackSizeResolver;
        }

        public override int MaxAccepted(Item item, int currentQuantity)
        {
            IContext context = Context.CreateContext(
                (ContextProperties.User, Owner),
                (ContextProperties.Inventory, Inventory),
                (ContextProperties.ItemToPutInInventory, item));
            int maxStackSize = StackSizeResolver.ResolveInt(0, context);
            return maxStackSize;
        }
    }
    internal class UniqueItemStackSizeModifier : IValueModifier<float>
    {
        public IModificationOutput<float> ModifyValue(IModificationInput<float> functionInput)
        {
            IContext context = functionInput.Context;
            if (!context.TryGetNonNull(ContextProperties.ItemToPutInInventory, out Item item)) return null;
            if (Item.IsRestrictedToSingleItem(item.Type)) return Output.BaseLevel(1, Localizer.DoStr("Base Level (unique item)"));
            return null;
        }
    }
}
