# Replacement Interactions

<u>*This is a tool for modders and does nothing by itself*</u>

Replace interaction methods and attributes without using override.cs files, and replace *some* RPCs (server data queried by the client) from outside the class. Also add, remove or modify interaction attributes for any existing interaction method.

### What's the motivation behind it?

Several mods override the same files to edit the interaction implementations or change the interaction parameters. Naturally they are not only incompatible with each other without the end user manually combining files, but also with any other modifications to unrelated sections of the file. By moving changes to interactions out of the override file it leaves the override file free for those other changes which cannot be done any other way.

There are also some limited use cases for changing what data is sent to back to the client which otherwise could not be done without modifying the server's source code.

## Usage:

### Replace an interaction method

If you want to make changes to an interaction method in a partial class you can do so without overriding the file, therefore making your mod more compatible with other mods.

In a separate file add your alternative implementation for the interaction to the partial class, with the `ReplacementInteraction` attribute instead of `Interaction`, where the parameter is the name of the method the interaction currently points to.

In this example this code would be in a separate file and not in ShovelItem.override.cs
```csharp
using ReplacementInteractions;
public partial class ShovelItem
{
	[ReplacementInteraction(nameof(ShovelItem.Dig))] //Transfer the "Dig" interaction here
	public bool Mod1Dig(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
	{
		//this method will get used instead of "Dig"
	}
}
```

### Add a new interaction to a method

You can add extra interaction parameters to existing methods from an external class using `AdditionalInteractionAttribute`. The attribute can be applied to methods of the form `public static InteractionAttribute Foo(Type interactorType)`, however the class also requires the attribute `DefinesInteractionsAttribute`. The method will be called for all of the interactor's derived types as well, although you can read the specific type from the argument passed in. Return either a new interaction to add to the method, or null.

The attribute's constructor is `AdditionalInteractionAttribute(Type interactorType, string method)`. The first parameter is the type of interactor e.g. `ShovelItem`, the second parameter is the name of the method within that class the interaction will execute. If the specified method does not exist or constitute a valid interaction it will be ignored. Like with all interactions, the method must be of the form `void/bool Foo(Player, InteractionTriggerInfo, InteractionTarget)` to be a valid interaction.

This example provides an additional way to dig using the "Interact" key (E).
```csharp
using ReplacementInteractions;
[DefinesInteractions]
public class MyAdditionalInteraction
{
	[AdditionalInteraction(typeof(ShovelItem), nameof(ShovelItem.Dig))]
	public static InteractionAttribute GetAdditionalInteraction(Type interactorType)
	{
		return new InteractionAttribute(InteractionTrigger.InteractKey);
	}
}
```

### Modify or remove an interaction

You can modify or remove interactions from an external class using `ModifyInteractionAttribute`. The attribute can be applied to methods of the form `public static void Foo(Type interactorType, ref InteractionAttribute interaction)`, however the class also requires the attribute `DefinesInteractionsAttribute`. The method will be called for all of the interactor's derived types as well, although you can read the specific type from the argument passed in. For interactions which are inherited from a base class, you can change the interaction for each child class independently. Either change the interaction parameters directly, replace the interaction with a new instance or set it to null to remove the interaction. Use `ReplacementInteractionAttibute` instead if you want to change which method it points to.

The attribute's constructor is `ModifyInteractionAttribute(Type interactorType, string method)`. The first parameter is the type of interactor e.g. `ShovelItem`, the second parameter is the name of the method within that class the interaction will execute. 

This example extends the distance you can pick items up from the ground.
```csharp
using ReplacementInteractions;
[DefinesInteractions]
public class FurtherReachExample
{
	//"Take" has two interactions, one for regular pick up and one for fast pick up, so this will get called for both.
	[ModifyInteraction(typeof(HandsInteractor), nameof(HandsInteractor.Take))] 
	public static void ModifyTakeInteraction(Type interactorType, ref InteractionAttribute interaction)
	{
		interaction.InteractionDistance += 15;
	}
}
```

### What if two mods want to replace the same method?

If two mods want to replace a method e.g. "Mod1Dig" and "Mod2Dig" both replace "Dig", then there must be a compatability patch for them to work together as interactions defined on the original methods must lead to only one implementation. That patch could take the form of a third method which is a combination of the two and transfers both "Mod1Dig" and "Mod2Dig" onto itself.
```csharp
using ReplacementInteractions;
public partial class ShovelItem
{
	[ReplacementInteraction("Mod1Dig")]
	[ReplacementInteraction("Mod2Dig")]
	public bool CompatabilityPatchMod1Mod2Dig(Player player, InteractionTriggerInfo triggerInfo, InteractionTarget target)
	{
		//The original "Dig" interaction now bypasses "Mod1Dig" and "Mod2Dig" to be replaced by this method
	}
}
```

### Replace an RPC from outside the class

You can redirect some (but not all) RPCs to your own implementations from an external class using `ReplacementRPCAttribute`. It works similarly to replacement interactions.

Your replacement method must be `public static`, and the class you put it in must have the `DefinesInteractionsAttribute`. The method will be only be called on the type you specify, not any of their derived types. Your method will be called with the class instance, followed by all of the method's arguments. It also needs the same return type.

The attribute's constructor is `ReplacementRPCAttribute(Type interactorType, string rpcName)`. The first parameter is the type of object the RPC is defined on, the second parameter is the name of the RPC method to replace. Replacements are not inherited, so a new attribute is needed for each of the target class' derived classes. Like interactions, you can also override other mods' replacements using additional attributes with the names of the other mods' replacement methods.

In this example, if there were a method `[RPC] int QueryData(string str)` in a class `ClassWithRPC` then this code would replace its implementation as far as the RPCManager is concerned. However, many RPCs seem to bypass this.
```csharp
[DefinesInteractions]
public class ExampleRPCReplacer
{
	[ReplacementRPC(typeof(ClassWithRPC), nameof(ClassWithRPC.QueryData))]
	[ReplacementRPC(typeof(ClassWithRPC), "NameOfOtherModsReplacementRPCMethod")] //take priority over another mod's replacement of the RPC, if it exists
	public static int ReplacementRPC(ClassWithRPC instance, string str)
	{
		return 1;
	}
}
```