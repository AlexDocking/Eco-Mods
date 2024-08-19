# Ecompatible (For Mod Developers)

The resolvers can be found in `Ecompatible.ValueResolvers`. The full list is below.

## Inventory

- `ValueResolvers.Inventory.User.Carried` (float).
	- Determines how many blocks the player can hold in the hands.
	- Value is rounded down.
	- Context should be at least:
		> User User;
		> Item ItemToPutInInventory;

## Shovel

- `ValueResolvers.Tools.Shovel.MaxTakeResolver` (float).
	- Can be used for changing how many blocks a shovel or player can dig whilst holding the shovel.
	- Value is rounded down.
	- Context should be at least:
		> User User;
		> Item ItemToPutInInventory;
		> ShovelItem Shovel;

## Pickaxe

- `ValueResolvers.Tools.Pickaxe.MiningSweepingHands.PickUpRangeResolver` (float).
	- Can change how far from the player rocks can be collected from when using the "Sweeping Hands" perk.
	- Value is rounded down.
	- Context should be at least:
		> User User;
		> Item ItemToPutInInventory;
		> MiningSweepingHandsTalent SweepingHands;

- Sweeping Hands will pick up as many rocks as possible up to the carry limit and not just the rock item's max stack size. In the vanilla game these are the same, but now multiple mods can apply modifiers to the carry limit and have them work together and be applied to sweeping hands.

## Axe

- `ValueResolvers.Tools.Axe.FractionOfTreeToSliceWhenFelled` (float).
	- Can be used to automatically slice up this fraction of the tree when it is felled.
	- Value should be between 0 and 1.
	- Context should be at least:
		> User User;
		> ToolItem ToolUsed;
		> TreeEntity Tree;

- `ValueResolvers.Tools.Axe.DamageToStumpWhenFelled` (float).
	- Can be used to automatically deal damage to the stump when the tree is felled.
	- TreeEntity now exposes `StumpHealth` property
	- Context should be at least:
		> User User;
		> ToolItem ToolUsed;
		> TreeEntity Tree;

- `ValueResolvers.Tools.Axe.MaxTreeDebrisToSpawn` (float).
	- Can be used to change the maximum number of debris that can spawn. It doesn't usually reach the default limit of 20 so increasing it is unlikely to do anything.
	- Value is rounded down.
	- Context should be at least:
		> User User;
		> ToolItem ToolUsed;
		> TreeEntity Tree;

- `ValueResolvers.Tools.Axe.ChanceToClearDebrisOnSpawn` (float).
	- Can be used to automatically clear debris as it spawns. Each piece of debris has a chance to be cleared before it actually spawns, when the trunk hits the ground. Calories, tool usage and experience still applies. If those checks fail then the debris will spawn as usual.
	- Value should be between 0 and 1.
	- Context should be at least:
		> User User;
		> ToolItem ToolUsed;
		> TreeEntity Tree;

- Without doing anything extra, players can also pick up split logs up to the carry limit and not just the log item's max stack size. In the vanilla game these are one and the same, but now multiple mods can apply modifiers to the carry limit and have them work together and be applied to picking up sliced logs.

## If you need to replace the "ShovelItem.Dig" implementation

If your mod wants to provide an alternative implementation of `ShovelItem.Dig`, put your new version in its own file with the attribute `[ReplacementInteraction("EcompatibleDig")]`. EcompatibleDig.cs replaces the vanilla implementation by redirecting the RPC call that shovels on the client make to the server, and your mod can do the same and redirect it again from EcompatibleDig to your own implementation.
See "Replacement Interations/README.md" for more details. 