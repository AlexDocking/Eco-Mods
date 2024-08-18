# Modding Guide

## Why use this?

1. If this mod caters for it, you don't need to manually patch compatability with other override mods. After all, we should want our mods to be compatible with each other.
2. If SLG change the vanilla implementation of the override files, your mod might not need updating because those changes will be made to the override files in this mod instead.

## How does Ecompatible work?

Ecompatible provides a way to resolve the value of each of its game variables. The game will use the resolver to determine the value instead of the original C# field or property. Mods are given the opportunity to change the output based on information provided by the calling code.

Each resolver is made up of a chain of modifiers which are inserted at specific positions in the sequence of modifiers. At runtime the resolver will pass the output of one modifier into the next, and so obtain a final result. Mods can then affect the output with no knowledge of each other.

The calling code will provide the resolver with relevant information about the context. Each modifier can then change the output independently of each other.

You can also resolve these values yourself in other scenarios such to create a tooltip, provided you supply the context values that the modifiers expect.

## How to apply a modifier to a resolver

To add a new modifier to one of the resolvers:
1. Create a new class that implements `IValueModifier<T>`.
	- `T` is the type of data to be resolved, and needs to be the same as the resolver. All the current resolvers use `float`.
	- The output may then get converted to other types afterwards such as rounding down a `float` to an `int`, but that is up to the code that calls the resolver.
2. The `Modify` function you need to write takes `IModificationInput<T>` and returns `IModificationOutput<T>`.
	- In the input object there is everything you need, including:
		- The output value from the previous step
		- A context object containing whatever data the source has provided in the way of context. Use the extension method `bool TryGetNonNull<T>(this IContext context, ContextKey key, out T value)` on the context to attempt to retrieve a non-null value from the context. For example `if(!context.TryGetNonNull(ContextProperties.User, out User user)) return null;` will let you skip any modification unless the context contains a `User` with the key `ContextProperties.User`.
	- *Note the implementation of this is subject to change.* To allow your modifier to be displayed in a table, output one of the following:
		1. `BaseLevelModificationOutput` - don't display any modifiers before this because this will overwrite their outputs.
		2. `OverwriteModificationOutput` - display previous modifiers but strike through their effects to show that they have been overwritten.
		3. `MultiplicationModificationOutput` - display the multiplier.
		4. `null` - no operation, don't display in a table.
		- If you return an output it also needs to contain a `LocString` to name your change. If you use one of the classes above they accept a LocString or string in their constructors.
3. Create a new server plugin class (or reuse an existing one if your mod already has one). It needs to be a class which implements `IModKitPlugin` and `IIinitializablePlugin`.
4. In the `Initialize()` method, tell the resolver to insert an instance of your new modifier and its priority in the list of modifiers.
	- For example, to add a modifier for the shovel "MaxTake" it would be `Ecompatible.ValueResolvers.Tools.Shovel.MaxTakeResolver.Add(priority, new MyModification());`.
	- The first parameter `priority` is a float, and determines the order in the chain where your modification will be applied. Use a larger number if you need your modification to be applied after everyone else's.
5. Test out your mod to make sure it all went well.