# Ecompatible Tools

Provides access points for other mods to change various game values at runtime related to tools, without each using override files which then have to be patched together.

This modding toolkit itself uses override files in order to facilitate this so that other mods can communicate with the Ecompatible API instead of each providing their own override files.

## Standalone Features

- This mod provides a server config where you can configure "Big Shovel" functionality.

- The tooltips for shovels will tell you how many blocks you can dig at one time with that shovel. If you're carrying a block the number will be specific for that type of block, in case mods change it depending on the type of block.

## Server Owners

### Ecompatible Shovel Config

You can configure this mod to act the same as the "Big Shovel" mod. The default values for how much each shovel type can pick up, as well as whether they should be affected by the server's stack size multiplier, can be set in the server config "EcompatibleShovel". Other mods can then make their changes based on those values.

If you are running your server on Windows then you can access this configuration file through the server GUI window that appears when you launch the server, otherwise copy the contents of "Configs/EcompatibleShovel.eco.template" to a new file "Configs/EcompatibleShovel.eco" and change the values in there.

### Installation:

Unzip and drop it in the server directory.

### Uninstallation

Go to Mods/UserCode and delete the following:
"Ecompatible Tools" (folder)
"Benefits/SweepingHands.override.cs"
"Objects/TreeObject.override.cs"

## Mod Developers

Why use this? Well, it means you don't need to manually patch compatability with other override mods that change how tools work, so long as this toolkit caters for it. Also you won't need to update override files when SLG changes them, because they will be updated here instead, and it is the intention of Ecompatible not to introduce breaking changes to your dependent mod except when absolutely necessary.

### How does it work?

Ecompatible provides a way to determine the value of each variable with a function call to a resolver instead of returning a fixed value as with a C# field or property. The calling code will provide the resolver with relevant information about the context, which then passes the initial value through a sequence of modifiers to obtain a final result.

To add a new modifier to one of the resolvers, create a new class that implements `IValueModifier<T>`. `T` is the type of data to be resolved, and needs to be the same as the resolver. It may then get converted to other types afterwards such as rounding down a `float` to an `int`, but that is up to the code that calls it. The `Modify` function you need to write takes `IModificationInput<T>` and returns `IModificationOutput<T>`. In the input object there is everything you need, including the output value from the previous step, and if you cast to the relevant type, potentially lots of useful data in the `Context` object.

In a server plugin, in the `Initialize()` method, tell the resolver to insert your new function. So, to add a modifier for the shovel "MaxTake" it would be `Ecompatible.Tools.Shovel.MaxTakeResolver.Add(priority, new MyModification());`. The first paramater `priority` is a float, and determines the order in the chain where your modification will be applied. Use a larger number if you need your modification to be applied after everyone else's.

### Shovel

- `Ecompatible.Tools.Shovel.MaxTakeResolver` (float).
	- Can be used for changing how many blocks a shovel or player can dig whilst holding the shovel. Value is rounded down. Uses `ShovelMaxTakeModificationContext` which has:
		> Item TargetItem (can be null)
		> ShovelItem Shovel

### Pickaxe

- `Ecompatible.Tools.Pickaxe.MaxStackSizeResolver` (float).
	- Can be used to change the pickup limit for stones when using the "Sweeping Hands" perk. Value is rounded down. Uses `SweepingHandsMaxTakeModificationContext` which has:
		> Item Resource
		> MiningSweepingHandsTalent SweepingHandsTalent

- `Ecompatible.Tools.Pickaxe.MiningSweepingHands.PickUpRangeResolver` (float).
	- Can change how far from the player rocks can be collected from when using the "Sweeping Hands" perk. Value is rounded down. Also uses `SweepingHandsMaxTakeModificationContext`.

### Axe
- `Ecompatible.Tools.Axe.MaxPickupResolver` (float).
	- Can be used to change the number of logs that can be held when picking up sliced up tree pieces. Value is rounded down. Uses `TreeEntityMaxPiclupModificationContext` which has:
		> TreeEntity Tree
		> AxeItem Axe

*All contexts also include the **User**, so you can provide different outputs for each player too.*

You can resolve these values yourself as well in other scenarios such to create a tooltip, provided you supply the relevant context.

See "Replacement Interations". 
Do get in touch if you have any problems or queries.
Enjoy!