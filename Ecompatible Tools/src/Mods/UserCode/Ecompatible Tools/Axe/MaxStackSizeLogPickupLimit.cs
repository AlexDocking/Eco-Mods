﻿using Eco.Gameplay.Items;
using Eco.Shared.Localization;
using EcompatibleTools;

namespace Ecompatible
{
    /// <summary>
    /// List of modifiers that change the max log pickup.
    /// </summary>
    /// <summary>
    /// Use the log's MaxStackSize as the default value
    /// </summary>
    public class MaxStackSizeLogPickupLimit : IValueModifier
    {
        public void ModifyValue(IValueModificationContext context, ref IOperationDetails operationDetails)
        {
            if (context is not TreeEntityMaxPickUpModificationContext treeContext) return;
            var resourceType = treeContext.Tree.Species.ResourceItemType;
            var resource = Item.Get(resourceType);
            context.FloatValue = context.IntValue = resource.MaxStackSize;
            operationDetails = new BaseLevelOperationDetails();
        }
    }
}