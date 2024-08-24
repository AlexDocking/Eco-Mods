# Ecompatible

Ecompatible provides access points for other mods to change various game values at runtime and on a per-player basis, without needing to be patched together.

This modding toolkit itself uses override files in order to facilitate this so that other mods can communicate with the Ecompatible API instead of each providing their own override files.

Modders should look first at "MODDING GUIDE.md" then "Tools/README - MODDERS.md".

## Standalone Features

- This mod provides a server config (EcompatibleShovel.eco) where you can set how many blocks each of the shovels should dig, so you can configure your own "Big Shovel" functionality. Other mods are free to apply modifiers on top of this.
- This mod also provides a server config (EcompatibleAxe.eco) where you can configure how trees get processed (auto sliced up, clear debris etc). Again, other mods are free to apply modifiers on these values too.
- The tooltips for shovels will tell you the limit for how many blocks you can dig with that shovel. If you're carrying a block the number will be specific for that type of block. Other mods may change the dig limit based on the player, shovel, block type, so the tooltip is there to display that in a table.
- Server Owners should look at "Tools/README - SERVER OWNERS.md" for help with the config.

## Dependencies:

- "Replacement Interactions" - is included in the installation

## Installation:

Unzip and drop it in the server directory.

## Uninstallation

Go to Mods/UserCode and delete the following:
"Ecompatible" (folder)
"Benefits/SweepingHands.override.cs"
"Objects/TreeObject.override.cs"

Do get in touch if you have any problems or queries.
Enjoy!