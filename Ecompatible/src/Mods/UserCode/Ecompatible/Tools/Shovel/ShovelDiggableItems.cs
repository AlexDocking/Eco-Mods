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
