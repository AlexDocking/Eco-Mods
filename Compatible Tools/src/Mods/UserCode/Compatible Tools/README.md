# Compatible Tools
Provides access points for other mods to modify various game values related to tools, without each using override files which then have to be patched together.

This mod uses override files in order to facilitate this so that other mods can communicate with the Compatible Tools API instead of each providing their own override files.

## Standalone Features
- This mod provides a server config where you can configure "Big Shovel" functionality.

- The tooltips for shovels will tell you how many blocks you can dig at one time with that shovel. If you're carrying a block the number will be specific for that type of block, in case mods change it depending on the type of block.

## Server Owners
### Compatible Shovel Config
You can configure this mod to act the same as the "Big Shovel" mod. The default values for how much each shovel type can pick up, as well as whether they should be affected by the server's stack size multiplier, can be set in the server config "CompatibleShovel". Other mods can then make their changes based on those values.

If you are running your server on Windows then you can access this configuration file through the server GUI window that appears when you launch the server, otherwise copy the contents of "Configs/CompatibleShovel.eco.template" to a new file "Configs/CompatibleShovel.eco" and change the values in there.

### Installation:
Unzip and drop it in the server directory.

### Uninstallation
Go to Mods/UserCode and delete the following:
"Compatible Tools" (folder)
"Benefits/SweepingHands.override.cs"
"Objects/TreeObject.override.cs"

## Mod Developers
Why use this? Well, it means you don't need to manually patch compatability with other override mods that change how tools work. Also you won't need to update override files when SLG changes them, because they will be updated here instead.

### How does it work?
Compatible Tools resolves the variables by passing them through a series of modifications, giving each the chance to change the value before passing it onto the next step. Each function also has access to useful information about the context.

To add a new modification to one of the variable, create a new class that implements `IModifyValueInPlaceHandler`. Cast the `context` argument to the relevant type so you can read the relevant information from it. The context's `FloatValue` and `IntValue` act as both the input and output for your function. Which of those outputs ultimately gets used depends on what the value is for, but by setting both it allows the next modification in the chain to work properly. For instance, multipliers should use `FloatValue` even if it will be `IntValue` that gets used as the final output. If yours is the last step, then you get to decide if the output gets rounded up or down.

Then, in a server plugin, in the `Initialize()` method, tell the variable to insert your new function. So, to add a modifier for `ShovelItem.MaxTakeResolver`, it would be `ShovelItem.MaxTakeResolver.Add(priority, new MyModification());`. The first paramater `priority` is a float, and determines the order in the chain where your modification will be applied. Use a larger number if you need your modification to be applied after everyone else's.

#### Shovel
You can change how many blocks a shovel or player can dig with `ShovelItem.MaxTakeResolver`. From the `ShovelMaxTakeModificationContext` you can get `Item TargetItem` and `ShovelItem Shovel`.
#### Pickaxe
You can change the pickup range and pickup limit for stones of the Sweeping Hands talent with `PickUpRangeResolver` and `MaxStackSizeResolver`. From the `SweepingHandsMaxTakeModificationContext` you can get `Item Resource` and `MiningSweepingHandsTalent SweepingHandsTalent`.
#### Axe
You can change the pickup limit for logs with `TreeObject.MaxPickupResolver`. From the `TreeEntityMaxPiclupModificationContext` you can get `TreeEntity Tree`, `float InitialPickup` and `AxeItem Axe`.

All contexts also include the user for whom the variable is being resolved, so you can make outputs different for each player.

You can also resolve these values in other contexts such as from a tooltip.

Do get in touch if you have any problems or queries.
Enjoy!