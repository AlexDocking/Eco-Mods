// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using Eco.Gameplay.Interactions.Interactors;
using Eco.Gameplay.Items;
using Eco.Shared.Networking;
using Eco.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Eco.Mods.TechTree
{
    public partial class ShovelItem
    {
        private static IDictionary<Type, Type[]> DiggableTypes { get; } = new ConcurrentDictionary<Type, Type[]>();
        private static void BuildDiggableItemsDictionary(Type shovelType)
        {
            if (DiggableTypes.ContainsKey(shovelType)) return;

            RPCManager.GetOrBuildLookup(shovelType).TryGetValue("Dig", out RPCMethod[] rpcs);
            foreach(var interaction in rpcs.Select(rpc => rpc.Attribute).OfType<InteractionAttribute>())
            {
                var blockTypes = interaction.TagsTargetable.SelectMany(tag => tag.TaggedTypes());
                var itemTypes = blockTypes.SelectNonNull(blockType => BlockItem.CreatingItem(blockType)?.Type);
                DiggableTypes.TryAdd(shovelType, itemTypes.ToArray());
            }
        }
        /// <summary>
        /// Returns whether digging up a block could produce this item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public bool IsDiggable(Type itemType)
        {
            BuildDiggableItemsDictionary(this.Type);
            return DiggableTypes[this.Type].Contains(itemType);
        }
    }
}
